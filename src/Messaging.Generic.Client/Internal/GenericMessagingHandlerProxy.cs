using Assistant.Net.Abstractions;
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Storage based message handling coordination proxy.
/// </summary>
internal class GenericMessagingHandlerProxy : IAbstractHandler
{
    private readonly ILogger logger;
    private readonly IOptionsSnapshot<GenericHandlerProxyOptions> options;
    private readonly IAdminStorage<string, RemoteHandlerModel> remoteHostRegistrationStorage;
    private readonly IPartitionedAdminStorage<string, IAbstractMessage> requestStorage;
    private readonly IAdminStorage<string, CachingResult> responseStorage;
    private readonly ITypeEncoder typeEncoder;
    private readonly ISystemClock clock;

    public GenericMessagingHandlerProxy(
        ILogger<GenericMessagingHandlerProxy> logger,
        IOptionsSnapshot<GenericHandlerProxyOptions> options,
        IAdminStorage<string, RemoteHandlerModel> remoteHostRegistrationStorage,
        IPartitionedAdminStorage<string, IAbstractMessage> requestStorage,
        IAdminStorage<string, CachingResult> responseStorage,
        ITypeEncoder typeEncoder,
        ISystemClock clock)
    {
        this.logger = logger;
        this.options = options;
        this.remoteHostRegistrationStorage = remoteHostRegistrationStorage;
        this.requestStorage = requestStorage;
        this.responseStorage = responseStorage;
        this.typeEncoder = typeEncoder;
        this.clock = clock;
    }

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token)
    {
        var strategy = options.Value.ResponsePoll;
        var attempt = 1;

        var messageName = typeEncoder.Encode(message.GetType());
        var messageId = message.GetSha1();

        var requestId = await PublishInternal(message, timeToLive: strategy.TotalTime, token);
        await Task.Delay(strategy.DelayTime(attempt), token);

        logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) polling: {Attempt} begins.",
            messageName, messageId, requestId, attempt);

        var watch = Stopwatch.StartNew();
        while (true)
        {
            token.ThrowIfCancellationRequested();

            if (await responseStorage.TryGet(requestId, token) is Some<CachingResult>(var response))
            {
                logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) polling: {Attempt} succeeded.",
                    messageName, messageId, requestId, attempt);
                return response.GetValue();
            }

            logger.LogWarning("Message({MessageName}, {MessageId}, {RequestId}) polling: {Attempt} ends without response.",
                messageName, messageId, requestId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Message({MessageName}, {MessageId}, {RequestId}) polling: {Attempt} reached the limit.",
                    messageName, messageId, requestId, attempt);
                throw new MessageDeferredException($"No response from server received in {watch.Elapsed}.");
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token) =>
        await PublishInternal(message, timeToLive: null, token);

    private async ValueTask<string> PublishInternal(IAbstractMessage message, TimeSpan? timeToLive, CancellationToken token)
    {
        var requestId = Guid.NewGuid().ToString();
        var messageName = typeEncoder.Encode(message.GetType())!;
        var messageId = message.GetSha1();

        if (await remoteHostRegistrationStorage.TryGet(messageName, token) is not Some<RemoteHandlerModel>({ HasRegistrations: true } model))
            throw new MessageNotRegisteredException(message.GetType());

        logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) publishing: begins.",
            messageName, messageId, requestId);

        var request = new StorageValue<IAbstractMessage>(message) {[MessagePropertyNames.RequestIdName] = requestId};
        if (timeToLive != null)
            request[MessagePropertyNames.ExpiredName] = clock.UtcNow.Add(timeToLive.Value).ToString("O");

        var instance = model.GetInstance()!;
        await requestStorage.Add(instance, request, token);

        logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) publishing: succeeded.",
            messageName, messageId, requestId);

        return requestId;
    }
}
