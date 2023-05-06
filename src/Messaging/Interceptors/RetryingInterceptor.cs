using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Retrying message handling interceptor.
/// </summary>
/// <remarks>
///     The interceptor depends on <see cref="MessagingClientOptions.TransientExceptions"/> and <see cref="MessagingClientOptions.Retry"/>.
/// </remarks>
public sealed class RetryingInterceptor : SharedAbstractInterceptor
{
    private readonly ILogger<RetryingInterceptor> logger;
    private readonly ITypeEncoder typeEncoder;
    private readonly IDiagnosticFactory diagnosticFactory;
    private readonly MessagingClientOptions options;

    /// <summary/>
    public RetryingInterceptor(
        ILogger<RetryingInterceptor> logger,
        ITypeEncoder typeEncoder,
        IDiagnosticFactory diagnosticFactory,
        INamedOptions<MessagingClientOptions> options)
    {
        this.logger = logger;
        this.typeEncoder = typeEncoder;
        this.diagnosticFactory = diagnosticFactory;
        this.options = options.Value;
    }

    /// <inheritdoc/>
    /// <exception cref="MessageRetryLimitExceededException"/>
    protected override async ValueTask<object> Intercept(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        var messageName = typeEncoder.Encode(message.GetType());

        var attempt = 1;
        while (true)
        {
            var operation = diagnosticFactory.Start($"{messageName}-handling-attempt-{attempt}");
            using var attemptScope = logger.BeginPropertyScope("RetryAttempt", attempt);

            logger.LogInformation("Message retrying: begins.");

            object response;
            try
            {
                response = await next(message, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                logger.LogWarning("Message retrying: cancelled.");
                throw;
            }
            catch (Exception ex) when (!options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
            {
                logger.LogError(ex, "Message retrying: rethrows permanent error.");
                operation.Fail();
                throw;
            }
            catch (Exception ex) when (!options.Retry.CanRetry(attempt + 1))
            {
                logger.LogError(ex, "Message retrying: reached the limit.");
                operation.Fail();
                throw new MessageRetryLimitExceededException();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Message retrying: failed on transient error.");

                operation.Fail();
                await Task.WhenAny(Task.Delay(options.Retry.DelayTime(attempt), token));
                attempt++;
                continue;
            }

            logger.LogInformation("Message retrying: ends.");
            operation.Complete();
            return response;
        }
    }
}
