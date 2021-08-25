using Assistant.Net.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Timeout tracking interceptor.
    /// </summary>
    public class TimeoutInterceptor : IMessageInterceptor
    {
        /// <inheritdoc/>
        public async Task<object> Intercept(IMessage<object> message, Func<IMessage<object>, Task<object>> next)
        {
            // todo: configurable (https://github.com/iotbusters/assistant.net/issues/4)
            var timeout = TimeSpan.FromSeconds(10);

            using var tokenSource = new CancellationTokenSource(timeout);
            return await next(message).ContinueWith(t => t.Result, tokenSource.Token);
        }
    }
}