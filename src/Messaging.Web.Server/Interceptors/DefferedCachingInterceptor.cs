using System;
using System.Linq;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Storage.Abstractions;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Deferred command response (including failures) caching interceptor.
    /// </summary>
    public sealed class DeferredCachingInterceptor : ICommandInterceptor
    {
        private readonly IStorage<object, DeferredCachingResult> deferredCache;

        public DeferredCachingInterceptor(IStorage<object, DeferredCachingResult> deferredCache) =>
            this.deferredCache = deferredCache;

        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            var deferredResult = await deferredCache.AddOrGet(command, _ => next(command));

            try
            {
                return await deferredResult.Get();
            }
            catch (Exception ex)
            {
                if(!IsCacheable(ex))
                    await deferredCache.TryRemove(command);
                throw;
            }
        }

        private static bool IsCacheable(Exception ex)
        {
            // todo: resolve duplication in ErrorHandlingInterceptor (https://github.com/iotbusters/assistant.net/issues/4)
            // configurable
            var noneCacheableExceptionTypes = new Type[]
            {
                typeof(TimeoutException),
                typeof(TaskCanceledException),
                typeof(OperationCanceledException),
                typeof(CommandDeferredException)
            };

            if (ex is AggregateException e)
                return IsCacheable(e.InnerException!);

            return !noneCacheableExceptionTypes.Any(x => x.IsAssignableFrom(ex.GetType()));
        }
    }
}