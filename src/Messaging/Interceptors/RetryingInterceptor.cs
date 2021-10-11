using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Utils;
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
            var strategy = options.Value.Retry;
            var transientExceptions = options.Value.TransientExceptions;
            var messageId = message.GetSha1();
            var messageName = message.GetType().Name.ToLower();

            var attempt = 1;
            while(true)
            {
                var operation = diagnosticFactory.Start($"{messageName}-handling-attempt-{attempt}");
                logger.LogDebug("Message({MessageName}/{MessageId}) retrying: {Attempt} begins.", messageName, messageId, attempt);

                try
                {
                    var response = await next(message, token);
                    logger.LogInformation("Message({MessageName}/{MessageId}) retrying: {Attempt} succeeded.", messageName, messageId, attempt);
                    operation.Complete();
                    return response;
                }
                catch (Exception ex)
                {
                    operation.Fail();
                    
                    if (!transientExceptions.Any(x => x.IsInstanceOfType(ex)))
                    {
                        logger.LogError(ex, "Message({MessageName}/{MessageId}) retrying: {Attempt} ends on permanent error.", messageName, messageId, attempt);
                        throw;
                    }

                    logger.LogWarning(ex, "Message({MessageName}/{MessageId}) retrying: {Attempt} ends on transient error.", messageName, messageId, attempt);

                    attempt++;
                    if (!strategy.CanRetry(attempt))
                    {
                        logger.LogError(ex, "Message({MessageName}/{MessageId}) retrying: {Attempt} won't proceed.", messageName, messageId, attempt);
                        break;
                    }
                }

                await Task.Delay(strategy.DelayTime(attempt), token);
            }

            throw new MessageRetryLimitExceededException();
        }
    }
}
