using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using Assistant.Net.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Message response (including failures) caching interceptor.
    /// </summary>
    public sealed class CachingInterceptor : IMessageInterceptor
    {
        private readonly IStorage<string, CachingResult> cache;

        /// <summary/>
        public CachingInterceptor(IStorage<string, CachingResult> cache) =>
            this.cache = cache;

        /// <inheritdoc/>
        public async Task<object> Intercept(
            Func<IMessage<object>, CancellationToken, Task<object>> next, IMessage<object> message, CancellationToken token)
        {
            var key = message.GetSha1();
            if (await cache.TryGet(key, token) is Some<CachingResult>(var cached))
                return cached.GetValue();

            return await next(message, token).When(
                completeAction: x => cache.AddOrGet(key, new CachingValueResult<dynamic>(x), token),
                faultAction: x =>
                {
                    if (IsCacheable(x)) cache.AddOrGet(key, new CachingExceptionResult(x), token);
                });
        }

        private static bool IsCacheable(Exception ex)
        {
            // todo: resolve duplication in ErrorHandlingInterceptor (https://github.com/iotbusters/assistant.net/issues/4)
            // configurable
            var transientExceptionTypes = new[]
            {
                typeof(TimeoutException),
                typeof(OperationCanceledException),
                typeof(MessageDeferredException)
            };

            if (ex is AggregateException e)
                return IsCacheable(e.InnerException!);

            return !transientExceptionTypes.Any(x => x.IsInstanceOfType(ex));
        }

    }
}