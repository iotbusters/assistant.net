using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Message response (including failures) caching interceptor.
    /// </summary>
    public sealed class CachingInterceptor : IMessageInterceptor
    {
        private readonly IStorage<object, CachingResult> cache;

        /// <summary/>
        public CachingInterceptor(IStorage<object, CachingResult> cache) =>
            this.cache = cache;

        /// <inheritdoc/>
        public async Task<object> Intercept(IMessage<object> message, Func<IMessage<object>, Task<object>> next)
        {
            if (await cache.TryGet(message) is Some<CachingResult>(var result))
                return result.GetValue();

            return await next(message).When(
                successAction: x => cache.AddOrGet(message, new CachingResult(x)),
                faultAction: x =>
                {
                    if (IsCacheable(x))
                        cache.AddOrGet(message, new CachingResult(x));
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