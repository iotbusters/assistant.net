using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Interceptors
{
    public sealed class CachingInterceptor : ICommandInterceptor
    {
        /// <summary>
        ///     Important logging. It should be persisted. Temporary solution.
        /// </summary>
        private readonly ConcurrentDictionary<string, Result> cache = new();

        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            var id = command.GetSha1();

            if (cache.TryGetValue(id, out var value))
                return value.Get();

            object response;
            try
            {
                response = await next(command);
            }
            catch (Exception ex)
            {
                cache.TryAdd(id, new Result(ex));
                throw;
            }

            cache.TryAdd(id, new Result(response));
            return response;
        }

        private class Result
        {
            private readonly object? value;
            private readonly Exception? exception;

            public Result(object value) => this.value = value;

            public Result(Exception exception) => this.exception = exception;

            public object Get()
            {
                if (exception != null)
                    ExceptionDispatchInfo.Capture(exception).Throw();
                return value!;
            }
        }
    }
}