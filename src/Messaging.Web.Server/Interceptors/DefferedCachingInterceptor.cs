using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Deferred message response (including failures) caching interceptor.
    /// </summary>
    public sealed class DeferredCachingInterceptor : IMessageInterceptor
    {
        private static readonly ConcurrentDictionary<string, DeferredCachingResult> DeferredCache = new();

        /// <inheritdoc/>
        public Task<object> Intercept(IMessage<object> message, Func<IMessage<object>, Task<object>> next)
        {
            var key = message.GetSha1();
            var task = next(message).WhenFaulted(x =>
            {
                if (!IsCacheable(x))
                    DeferredCache.TryRemove(key, out _);
            });
            return DeferredCache.GetOrAdd(key, _ => task).GetTask();
        }

        private static bool IsCacheable(Exception ex)
        {
            // todo: resolve duplication in ErrorHandlingInterceptor (https://github.com/iotbusters/assistant.net/issues/4)
            // configurable
            var transientExceptionTypes = new Type[]
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