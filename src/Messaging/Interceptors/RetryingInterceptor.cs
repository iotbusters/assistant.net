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
public sealed class RetryingInterceptor : IAbstractInterceptor
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
    public async Task<object> Intercept(Func<IAbstractMessage, CancellationToken, Task<object>> next, IAbstractMessage message, CancellationToken token)
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
                await Task.WhenAll(Task.Delay(options.Retry.DelayTime(attempt), token));
                attempt++;
                continue;
            }

            logger.LogInformation("Message({MessageName}/{MessageId}) retrying: {Attempt} succeeded.", messageName, messageId, attempt);
            operation.Complete();
            return response;
        }
    }
}

/// <inheritdoc cref="RetryingInterceptor"/>
public sealed class RetryingInterceptor<TMessage, TResponse> : IMessageInterceptor<TMessage, TResponse>
    where TMessage : IMessage<TResponse>
{
    private readonly RetryingInterceptor interceptor;

    /// <summary/>
    public RetryingInterceptor(
        ILogger<RetryingInterceptor> logger,
        ITypeEncoder typeEncoder,
        IDiagnosticFactory diagnosticFactory,
        INamedOptions<MessagingClientOptions> options)
    {
        this.interceptor = new RetryingInterceptor(logger, typeEncoder, diagnosticFactory, options);
    }

    /// <inheritdoc/>
    /// <exception cref="MessageRetryLimitExceededException"/>
    public async Task<TResponse> Intercept(MessageInterceptor<TMessage, TResponse> next, TMessage message, CancellationToken token) =>
        (TResponse)await interceptor.Intercept(async (m, t) => (await next((TMessage)m, t))!, message, token);
}
