using System;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Timeout tracking interceptor.
    /// </summary>
    public class TimeoutInterceptor : ICommandInterceptor
    {
        public async Task<object> Intercept(ICommand<object> command, Func<ICommand<object>, Task<object>> next)
        {
            // todo: configurable (https://github.com/iotbusters/assistant.net/issues/4)
            var timeout = TimeSpan.FromSeconds(30);

            return await Task.Run(() => next(command), new CancellationTokenSource(timeout).Token);
        }
    }
}