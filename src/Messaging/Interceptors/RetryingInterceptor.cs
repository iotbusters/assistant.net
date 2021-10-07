using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <inheritdoc cref="RetryingInterceptor{TMessage,TResponse}"/>
    public class RetryingInterceptor : RetryingInterceptor<IMessage<object>, object>, IMessageInterceptor
    {
        /// <summary/>
        public RetryingInterceptor(ILogger<RetryingInterceptor> logger, IDiagnosticFactory diagnosticFactory, IOptions<MessagingClientOptions> options) : base(logger, diagnosticFactory, options) { }
    }

    /// <summary>
    ///     Retrying message handling interceptor.
    /// </summary>
    /// <remarks>
    ///     The interceptor depends on <see cref="MessagingClientOptions.TransientExceptions"/> and <see cref="MessagingClientOptions.Retry"/>.
    /// </remarks>
    public class RetryingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
        where TMessage : IMessage<TResponse>
    {
        private readonly ILogger<RetryingInterceptor> logger;
        private readonly IDiagnosticFactory diagnosticFactory;
        private readonly IOptions<MessagingClientOptions> options;

        /// <summary/>
        public RetryingInterceptor(
            ILogger<RetryingInterceptor> logger,
            IDiagnosticFactory diagnosticFactory,
            IOptions<MessagingClientOptions> options)
        {
            this.logger = logger;
            this.diagnosticFactory = diagnosticFactory;
            this.options = options;
        }

        /// <inheritdoc/>
        /// <exception cref="MessageRetryLimitExceededException" />
        public async Task<TResponse> Intercept(Func<TMessage, CancellationToken, Task<TResponse>> next, TMessage message, CancellationToken token)
        {
            var clientOptions = options.Value;

            var messageName = message.GetType().Name.ToLower();
            var attempt = 1;

            while(!token.IsCancellationRequested)
            {
                var operation = diagnosticFactory.Start($"{messageName}-handling-attempt-{attempt}");
                logger.LogInformation("Operation {Attempt} has been started.", attempt);

                try
                {
                    var response = await next(message, token);
                    operation.Complete();
                    return response;
                }
                catch (Exception ex)
                {
                    operation.Fail();

                    if (!clientOptions.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
                    {
                        logger.LogError(ex, "Permanent error occurred during {Attempt}.", attempt);
                        throw;
                    }

                    logger.LogWarning(ex, "Transient error occurred during {Attempt}.", attempt);

                    if (!clientOptions.Retry.CanRetry(attempt))
                    {
                        logger.LogError(ex, "Retrying strategy failed after {Attempt}.", attempt);
                        break;
                    }
                }

                await Task.Delay(clientOptions.Retry.DelayTime(attempt), token);
                attempt++;
            }

            throw new MessageRetryLimitExceededException();
        }
    }
}
