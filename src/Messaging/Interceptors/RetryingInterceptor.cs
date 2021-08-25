using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Retrying message handling interceptor.
    /// </summary>
    public class RetryingInterceptor : IMessageInterceptor
    {
        private readonly ILogger<RetryingInterceptor> logger;
        private readonly IDiagnosticFactory diagnosticsFactory;
        private readonly ISystemLifetime lifetime;

        /// <summary/>
        public RetryingInterceptor(
            ILogger<RetryingInterceptor> logger,
            IDiagnosticFactory diagnosticsFactory,
            ISystemLifetime lifetime)
        {
            this.logger = logger;
            this.diagnosticsFactory = diagnosticsFactory;
            this.lifetime = lifetime;
        }

        /// <inheritdoc/>
        /// <exception cref="MessageRetryLimitExceededException" />
        public async Task<object> Intercept(IMessage<object> message, Func<IMessage<object>, Task<object>> next)
        {
            // configurable
            var maxRetryLimit = 5;
            var delayingStrategy = new Func<int, TimeSpan>(x => TimeSpan.FromSeconds(Math.Pow(x, 2)));
            var breakStrategy = new Func<Exception, bool>(CriticalExceptionOnly);

            for (var attempt = 1; attempt <= maxRetryLimit; attempt++)
            {
                var operation = diagnosticsFactory.Start($"retry-attempt-{attempt}");

                lifetime.Stopping.ThrowIfCancellationRequested();

                try
                {
                    return await next(message);
                }
                catch (Exception ex)
                {
                    operation.Fail();

                    if (breakStrategy(ex))
                        throw;

                    logger.LogWarning(ex, "#{Attempt}. Transient error occurred.", attempt);
                    await Task.Delay(delayingStrategy(attempt));
                }
                finally
                {
                    operation.Complete();
                }
            }

            throw new MessageRetryLimitExceededException();
        }

        private static bool CriticalExceptionOnly(Exception ex)
        {
            // todo: resolve duplication in ErrorHandlingInterceptor (https://github.com/iotbusters/assistant.net/issues/4)
            // configurable
            var transientExceptionTypes = new[]
            {
                typeof(MessageDeferredException)
            };

            if (ex is AggregateException e)
                return CriticalExceptionOnly(e.InnerException!);

            return !transientExceptionTypes.Any(x => x.IsInstanceOfType(ex));
        }
    }
}