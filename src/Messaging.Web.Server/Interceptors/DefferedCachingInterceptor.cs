using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Deferred message response (including failures) caching interceptor.
    /// </summary>
    public sealed class DeferredCachingInterceptor : IMessageInterceptor
    {
        private static readonly ConcurrentDictionary<string, DeferredCachingResult> deferredCache = new();

        /// <inheritdoc/>
        public Task<object> Intercept(
            Func<IMessage<object>, CancellationToken, Task<object>> next, IMessage<object> message, CancellationToken token)
        {
            var key = message.GetSha1();
            return deferredCache.GetOrAdd(key, _ => next(message, token).WhenFaulted(x =>
            {
                if (!IsCacheable(x))
                    deferredCache.TryRemove(key, out var _);
            })).GetTask();
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
