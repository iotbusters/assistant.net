﻿using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
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
internal class GenericMessagingHandlerProxy : IAbstractHandler
{
    private readonly ILogger logger;
    private readonly IOptionsSnapshot<GenericHandlerProxyOptions> options;
    private readonly IPartitionedStorage<int, IAbstractMessage> requestStorage;
    private readonly IAdminStorage<IAbstractMessage, CachingResult> responseStorage;
    private readonly ITypeEncoder typeEncoder;

    public GenericMessagingHandlerProxy(
        ILogger<GenericMessagingHandlerProxy> logger,
        IOptionsSnapshot<GenericHandlerProxyOptions> options,
        IPartitionedStorage<int, IAbstractMessage> requestStorage,
        IAdminStorage<IAbstractMessage, CachingResult> responseStorage,
        ITypeEncoder typeEncoder)
    {
        this.logger = logger;
        this.options = options;
        this.requestStorage = requestStorage;
        this.responseStorage = responseStorage;
        this.typeEncoder = typeEncoder;
    }

    public async Task<object> Request(object message, CancellationToken token)
    {
        var clientOptions = options.Value;
        var strategy = clientOptions.ResponsePoll;
        var attempt = 1;

        var messageName = typeEncoder.Encode(message.GetType())
                          ?? throw new NotSupportedException($"Not supported  message type '{message.GetType()}'.");
        var messageId = message.GetSha1();

        await Publish(message, token);
        await Task.Delay(strategy.DelayTime(attempt), token);

        logger.LogDebug("Message({MessageName}/{MessageId}) polling: {Attempt} begins.", messageName, messageId, attempt);

        while (!token.IsCancellationRequested)
        {
            if (await responseStorage.TryGet((IAbstractMessage)message, token) is Some<CachingResult>(var response))
            {
                logger.LogInformation("Message({MessageName}/{MessageId}) polling: {Attempt} ends with response.", messageName, messageId, attempt);
                return response.GetValue();
            }

            logger.LogInformation("Message({MessageName}/{MessageId}) polling: {Attempt} ends without response.", messageName, messageId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogInformation("Message({MessageName}/{MessageId}) polling: {Attempt} won't proceed.", messageName, messageId, attempt);
                break;
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        throw new MessageDeferredException("No response from server in defined amount of time.");
    }

    public async Task Publish(object message, CancellationToken token)
    {
        var clientOptions = options.Value;

        await requestStorage.Add(clientOptions.InstanceId, (IAbstractMessage)message, token);

        var messageName = typeEncoder.Encode(message.GetType())
                          ?? throw new NotSupportedException($"Not supported  message type '{message.GetType()}'.");
        var messageId = message.GetSha1();
        logger.LogDebug("Message({MessageName}/{MessageId}) publishing: succeeded.", messageName, messageId);
    }
}