using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

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
            var timeout = TimeSpan.FromSeconds(10);

            using var tokenSource = new CancellationTokenSource(timeout);
            return await next(command).ContinueWith(t => t.Result, tokenSource.Token);
        }
    }
}