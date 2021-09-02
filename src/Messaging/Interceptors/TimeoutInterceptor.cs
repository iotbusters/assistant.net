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
        public async Task<object> Intercept(
            Func<IMessage<object>, CancellationToken, Task<object>> next, IMessage<object> message, CancellationToken token)
        {
            // todo: configurable (https://github.com/iotbusters/assistant.net/issues/4)
            var timeout = TimeSpan.FromSeconds(10);

            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                new CancellationTokenSource(timeout).Token,
                token);

            return await next(message, token);
        }
    }
}