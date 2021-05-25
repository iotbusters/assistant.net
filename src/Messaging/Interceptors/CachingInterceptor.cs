using System;
using System.Linq;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Caching;
using Assistant.Net.Storage.Abstractions;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Command response (including failures) caching interceptor.
    /// </summary>
    public sealed class CachingInterceptor : ICommandInterceptor
    {
        private readonly IStorage<object, Result> cache;

        public CachingInterceptor(IStorage<object, Result> cache) =>
            this.cache = cache;

        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            var result = await cache.AddOrGet(command, async _ =>
            {
                try
                {
                    return new Result(await next(command));
                }
                catch (Exception ex)
                {
                    if(IsCritical(ex))
                        throw;
                    return new Result(ex);
                }
            });
            return result.Get();
        }

        private static bool IsCritical(Exception ex)
        {
            // todo: resolve duplication in ErrorHandlingInterceptor (https://github.com/iotbusters/assistant.net/issues/4)
            // configurable
            var criticalExceptionTypes = new Type[]
            {
                typeof(TimeoutException),
                typeof(TaskCanceledException),
                typeof(OperationCanceledException),
                typeof(CommandDeferredException)
            };

            if (ex is AggregateException e)
                return IsCritical(e.InnerException!);

            return criticalExceptionTypes.Any(x => x.IsAssignableFrom(ex.GetType()));
        }
    }
}