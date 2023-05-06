using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Storage based message handling coordination proxy.
/// </summary>
internal class GenericMessageHandlerProxy : IAbstractHandler
{
    private readonly ILogger<GenericMessageHandlerProxy> logger;
    private readonly IRetryStrategy responsePollStrategy;
    private readonly IHostSelectionStrategy hostSelectionStrategy;
    private readonly IPartitionedAdminStorage<string, IAbstractMessage> requestStorage;
    private readonly IStorage<string, CachingResult> responseStorage;
    private readonly IDiagnosticContext context;
    private readonly ISystemClock clock;

    public GenericMessageHandlerProxy(
        ILogger<GenericMessageHandlerProxy> logger,
        INamedOptions<GenericHandlerProxyOptions> options,
        IServiceProvider provider,
        IPartitionedAdminStorage<string, IAbstractMessage> requestStorage,
        IStorage<string, CachingResult> responseStorage,
        IDiagnosticContext context,
        ISystemClock clock)
    {
        this.logger = logger;
        this.responsePollStrategy = options.Value.ResponsePoll;
        this.hostSelectionStrategy = options.Value.HostSelectionStrategyFactory.Create(provider);
        this.requestStorage = requestStorage;
        this.responseStorage = responseStorage;
        this.context = context;
        this.clock = clock;
    }

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token)
    {
        using var requestScope = logger
            .BeginPropertyScope()
            .AddPropertyScope("CorrelationId", context.CorrelationId!)
            .AddPropertyScope("User", context.User!);

        var requestId = await PublishInternal(message, timeToLive: responsePollStrategy.TotalTime, token);
        requestScope.AddPropertyScope("RequestId", requestId);

        var attempt = 1;
        while (responsePollStrategy.CanRetry(attempt))
        {
            await Task.Delay(responsePollStrategy.DelayTime(attempt), token);

            using var attemptScope = logger.BeginPropertyScope("PollAttempt", attempt);
            logger.LogDebug("Response polling attempt: begins.");

            if (await responseStorage.TryGet(requestId, token) is Some<CachingResult>(var response))
            {
                logger.LogInformation("Response polling attempt: ends.");
                return response.GetValue();
            }

            logger.LogWarning("Response polling attempt: ends without a response.");
            attempt++;
        }

        logger.LogError("Response polling attempt: reached the limit.");
        throw new MessageDeferredException("No response from server received.");
    }

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token) =>
        await PublishInternal(message, timeToLive: null, token);

    private async ValueTask<string> PublishInternal(IAbstractMessage message, TimeSpan? timeToLive, CancellationToken token)
    {
        var requestId = Guid.NewGuid().ToString();
        var messageType = message.GetType();
        var instanceName = await hostSelectionStrategy.GetInstance(messageType, token)
                           ?? throw new MessageNotRegisteredException(messageType);

        using var _ = logger.BeginPropertyScope("RequestId", requestId);
        logger.LogDebug("Message publish: begins.");

        var request = new StorageValue<IAbstractMessage>(message) {[MessagePropertyNames.RequestIdName] = requestId};
        if (timeToLive != null)
            request[MessagePropertyNames.ExpiredName] = clock.UtcNow.Add(timeToLive.Value).ToString("O");

        await requestStorage.Add(instanceName, request, token);

        logger.LogDebug("Message publish: ends.");
        return requestId;
    }
}
