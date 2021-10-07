using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <inheritdoc cref="DiagnosticsInterceptor{TMessage,TResponse}"/>
    public class ErrorHandlingInterceptor : ErrorHandlingInterceptor<IMessage<object>, object>, IMessageInterceptor
    {
        /// <summary/>
        public ErrorHandlingInterceptor(IOptions<MessagingClientOptions> options) : base(options) { }
    }

    /// <summary>
    ///     Global error handling interceptor.
    /// </summary>
    /// <remarks>
    ///     The interceptor depends on <see cref="MessagingClientOptions.ExposedExceptions"/>
    /// </remarks>
    public class ErrorHandlingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
        where TMessage : IMessage<TResponse>
    {
        private readonly IOptions<MessagingClientOptions> options;

        /// <summary/>
        public ErrorHandlingInterceptor(IOptions<MessagingClientOptions> options) =>
            this.options = options;

        /// <inheritdoc/>
        public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
        {
            var clientOptions = options.Value;

            try
            {
                return await next(message, token);
            }
            catch (Exception ex)
            {
                if (ex is MessageException || clientOptions.ExposedExceptions.Contains(ex.GetType()))
                    throw;
                throw new MessageFailedException(ex);
            }
            
        }
    }
}
