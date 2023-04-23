using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Storage based message handling coordination proxy.
/// </summary>
internal class GenericMessageHandlerProxy : IAbstractHandler
{
    private readonly ILogger logger;
    private readonly IOptionsSnapshot<GenericHandlerProxyOptions> options;
    private readonly IAdminStorage<string, RemoteHandlerModel> remoteHostRegistrationStorage;
    private readonly IPartitionedAdminStorage<string, IAbstractMessage> requestStorage;
    private readonly IAdminStorage<string, CachingResult> responseStorage;
    private readonly IDiagnosticContext context;
    private readonly ITypeEncoder typeEncoder;
    private readonly ISystemClock clock;

    public GenericMessageHandlerProxy(
        ILogger<GenericMessageHandlerProxy> logger,
        IOptionsSnapshot<GenericHandlerProxyOptions> options,
        IAdminStorage<string, RemoteHandlerModel> remoteHostRegistrationStorage,
        IPartitionedAdminStorage<string, IAbstractMessage> requestStorage,
        IAdminStorage<string, CachingResult> responseStorage,
        IDiagnosticContext context,
        ITypeEncoder typeEncoder,
        ISystemClock clock)
    {
        this.logger = logger;
        this.options = options;
        this.remoteHostRegistrationStorage = remoteHostRegistrationStorage;
        this.requestStorage = requestStorage;
        this.responseStorage = responseStorage;
        this.context = context;
        this.typeEncoder = typeEncoder;
        this.clock = clock;
    }

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token)
    {
        var strategy = options.Value.ResponsePoll;

        var messageName = typeEncoder.Encode(message.GetType());
        var messageId = message.GetSha1();
        using var requestScope = logger
            .BeginPropertyScope()
            .AddPropertyScope("CorrelationId", context.CorrelationId!)
            .AddPropertyScope("User", context.User!)
            .AddPropertyScope("MessageId", messageId)
            .AddPropertyScope("MessageName", messageName!);

        var requestId = await PublishInternal(message, timeToLive: strategy.TotalTime, token);
        requestScope.AddPropertyScope("RequestId", requestId);

        var attempt = 1;
        while (strategy.CanRetry(attempt))
        {
            await Task.Delay(strategy.DelayTime(attempt), token);

            using var attemptScope = logger.BeginPropertyScope("Attempt", attempt);
            logger.LogDebug("Poll response: begins.");

            if (await responseStorage.TryGet(requestId, token) is Some<CachingResult>(var response))
            {
                logger.LogInformation("Poll response: ends.");
                return response.GetValue();
            }

            logger.LogWarning("Poll response: ends without response.");
            attempt++;
        }

        logger.LogError("Poll response: reached the limit.");
        throw new MessageDeferredException("No response from server received.");
    }

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token) =>
        await PublishInternal(message, timeToLive: null, token);

    private async ValueTask<string> PublishInternal(IAbstractMessage message, TimeSpan? timeToLive, CancellationToken token)
    {
        var requestId = Guid.NewGuid().ToString();
        var messageName = typeEncoder.Encode(message.GetType())!;

        if (await remoteHostRegistrationStorage.TryGet(messageName, token) is not Some<RemoteHandlerModel>({ HasRegistrations: true } model))
            throw new MessageNotRegisteredException(message.GetType());

        using var scope = logger.BeginPropertyScope("RequestId", requestId);
        logger.LogDebug("Publish: begins.");

        var request = new StorageValue<IAbstractMessage>(message) {[MessagePropertyNames.RequestIdName] = requestId};
        if (timeToLive != null)
            request[MessagePropertyNames.ExpiredName] = clock.UtcNow.Add(timeToLive.Value).ToString("O");

        var instance = model.GetInstance()!;
        await requestStorage.Add(instance, request, token);

        logger.LogInformation("Publish: ends.");
        return requestId;
    }
}
