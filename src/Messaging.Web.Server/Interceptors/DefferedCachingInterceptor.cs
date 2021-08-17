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
    ///     Deferred command response (including failures) caching interceptor.
    /// </summary>
    public sealed class DeferredCachingInterceptor : ICommandInterceptor
    {
        private static readonly ConcurrentDictionary<string, DeferredCachingResult> deferredCache = new();

        /// <inheritdoc/>
        public Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            var key = command.GetSha1();
            var task = next(command).WhenFaulted(x =>
            {
                if (!IsCacheable(x))
                    deferredCache.TryRemove(key, out _);
            });
            return deferredCache.GetOrAdd(key, _ => task).GetTask();
        }

        private static bool IsCacheable(Exception ex)
        {
            // todo: resolve duplication in ErrorHandlingInterceptor (https://github.com/iotbusters/assistant.net/issues/4)
            // configurable
            var transientExceptionTypes = new Type[]
            {
                typeof(TimeoutException),
                typeof(OperationCanceledException),
                typeof(CommandDeferredException)
            };

            if (ex is AggregateException e)
                return IsCacheable(e.InnerException!);

            return !transientExceptionTypes.Any(x => x.IsInstanceOfType(ex));
        }
    }
}