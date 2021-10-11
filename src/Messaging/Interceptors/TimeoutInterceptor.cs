using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <inheritdoc cref="TimeoutInterceptor{TMessage,TResponse}"/>
    public class TimeoutInterceptor : TimeoutInterceptor<IMessage<object>, object>, IMessageInterceptor
    {
        /// <summary/>
        public TimeoutInterceptor(IOptions<MessagingClientOptions> options) : base(options) { }
    }

    /// <summary>
    ///     Timeout tracking interceptor.
    /// </summary>
    /// <remarks>
    ///     The interceptor depends on <see cref="MessagingClientOptions.Timeout"/>
    /// </remarks>
    public class TimeoutInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
        where TMessage : IMessage<TResponse>
    {
        private readonly MessagingClientOptions options;

        /// <summary/>
        public TimeoutInterceptor(IOptions<MessagingClientOptions> options) =>
            this.options = options.Value;

        /// <inheritdoc/>
        public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
        {
            using var newSource = CancellationTokenSource.CreateLinkedTokenSource(
                new CancellationTokenSource(options.Timeout).Token,
                token);

            return await next(message, newSource.Token);
        }
    }
}
