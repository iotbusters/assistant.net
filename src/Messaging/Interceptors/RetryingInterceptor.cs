using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Utils;
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
    protected override async ValueTask<object> InterceptInternal(SharedMessageHandler next, IAbstractMessage message, CancellationToken token)
    {
        var messageId = message.GetSha1();
        var messageName = typeEncoder.Encode(message.GetType());

        var attempt = 1;
        while (true)
        {
            var operation = diagnosticFactory.Start($"{messageName}-handling-attempt-{attempt}");
            logger.LogInformation("Message({MessageName}/{MessageId}) retrying: {Attempt} begins.", messageName, messageId, attempt);

            object response;
            try
            {
                response = await next(message, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                logger.LogWarning("Message({MessageName}/{MessageId}) retrying: {Attempt} cancelled.",
                    messageName, messageId, attempt);
                throw;
            }
            catch (Exception ex) when (!options.TransientExceptions.Any(x => x.IsInstanceOfType(ex)))
            {
                logger.LogError(ex, "Message({MessageName}/{MessageId}) retrying: {Attempt} rethrows permanent error.",
                    messageName, messageId, attempt);
                operation.Fail();
                throw;
            }
            catch (Exception ex) when (!options.Retry.CanRetry(attempt + 1))
            {
                logger.LogError(ex, "Message({MessageName}/{MessageId}) retrying: {Attempt} reached the limit.",
                    messageName, messageId, attempt);
                operation.Fail();
                throw new MessageRetryLimitExceededException();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Message({MessageName}/{MessageId}) retrying: {Attempt} failed on transient error.",
                    messageName, messageId, attempt);

                operation.Fail();
                await Task.WhenAny(Task.Delay(options.Retry.DelayTime(attempt), token));
                attempt++;
                continue;
            }

            logger.LogInformation("Message({MessageName}/{MessageId}) retrying: {Attempt} succeeded.", messageName, messageId, attempt);
            operation.Complete();
            return response;
        }
    }
}
