using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Command response (including failures) caching interceptor.
    /// </summary>
    public sealed class CachingInterceptor : ICommandInterceptor
    {
        private readonly IStorage<object, CachingResult> cache;

        public CachingInterceptor(IStorage<object, CachingResult> cache) =>
            this.cache = cache;

        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next) =>
            await next(command).When(
                x => cache.AddOrGet(command, new CachingResult(x)),
                x =>
                {
                    if (IsCacheable(x))
                        cache.AddOrGet(command, new CachingResult(x));
                });

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

            return !transientExceptionTypes.Any(x => x.IsAssignableFrom(ex.GetType()));
        }

    }
}