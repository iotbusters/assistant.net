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
    private readonly IPartitionedAdminStorage<int, IAbstractMessage> requestStorage;
    private readonly IAdminStorage<string, CachingResult> responseStorage;
    private readonly ITypeEncoder typeEncoder;

    public GenericMessagingHandlerProxy(
        ILogger<GenericMessagingHandlerProxy> logger,
        IOptionsSnapshot<GenericHandlerProxyOptions> options,
        IPartitionedAdminStorage<int, IAbstractMessage> requestStorage,
        IAdminStorage<string, CachingResult> responseStorage,
        ITypeEncoder typeEncoder)
    {
        this.logger = logger;
        this.options = options;
        this.requestStorage = requestStorage;
        this.responseStorage = responseStorage;
        this.typeEncoder = typeEncoder;
    }

    public async ValueTask<object> Request(IAbstractMessage message, CancellationToken token)
    {
        var clientOptions = options.Value;
        var strategy = clientOptions.ResponsePoll;
        var attempt = 1;

        var messageName = typeEncoder.Encode(message.GetType());
        var messageId = message.GetSha1();

        var requestId = await PublishInternal(message, token);
        await Task.Delay(strategy.DelayTime(attempt), token);

        logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) polling: {Attempt} begins.",
            messageName, messageId, requestId, attempt);

        var watch = Stopwatch.StartNew();
        while (true)
        {
            token.ThrowIfCancellationRequested();

            if (await responseStorage.TryGet(requestId, token) is Some<CachingResult>(var response))
            {
                logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) polling: {Attempt} ends with response.",
                    messageName, messageId, requestId, attempt);
                return response.GetValue();
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Message({MessageName}, {MessageId}, {RequestId}) polling: {Attempt} reached the limit.",
                    messageName, messageId, requestId, attempt);
                throw new MessageDeferredException($"No response from server received in {watch.Elapsed}.");
            }

            logger.LogWarning("Message({MessageName}, {MessageId}, {RequestId}) polling: {Attempt} ends without response.",
                messageName, messageId, requestId, attempt);
            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async ValueTask Publish(IAbstractMessage message, CancellationToken token) =>
        await PublishInternal(message, token);

    private async ValueTask<string> PublishInternal(IAbstractMessage message, CancellationToken token)
    {
        var clientOptions = options.Value;

        var requestId = Guid.NewGuid().ToString();
        var messageName = typeEncoder.Encode(message.GetType());
        var messageId = message.GetSha1();

        logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) publishing: begins.",
            messageName, messageId, requestId);

        var request = new StorageValue<IAbstractMessage>(message) {[MessagePropertyNames.RequestIdName] = requestId};
        await requestStorage.Add(clientOptions.InstanceId, request, token);

        logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) publishing: succeeded.",
            messageName, messageId, requestId);

        return requestId;
    }
}
