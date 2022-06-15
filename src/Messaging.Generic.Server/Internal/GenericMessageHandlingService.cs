﻿using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Unions;
using Assistant.Net.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Storage based message handling service.
/// </summary>
internal class GenericMessageHandlingService : BackgroundService
{
    private readonly ILogger<GenericMessageHandlingService> logger;
    private readonly IOptionsMonitor<GenericHandlingServerOptions> options;
    private readonly ITypeEncoder typeEncoder;
    private readonly IServiceScopeFactory scopeFactory;

    public GenericMessageHandlingService(
        ILogger<GenericMessageHandlingService> logger,
        IOptionsMonitor<GenericHandlingServerOptions> options,
        ITypeEncoder typeEncoder,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.options = options;
        this.typeEncoder = typeEncoder;
        this.scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await using var mainScope = scopeFactory.CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        var provider = mainScope.ServiceProvider;
        var requestStorage = provider.GetRequiredService<IPartitionedAdminStorage<int, IAbstractMessage>>();
        var processedIndexStorage = provider.GetRequiredService<IStorage<int, long>>();
        var responseStorage = provider.GetRequiredService<IStorage<IAbstractMessage, CachingResult>>();

        var serverOptions = options.CurrentValue;
        var index = await processedIndexStorage.GetOrDefault(serverOptions.InstanceId, token) + 1;

        while (!token.IsCancellationRequested)
        {
            logger.LogDebug("#{Index:D5}: Find next message.", index);

            if (await requestStorage.TryGet(serverOptions.InstanceId, index, token) is not Some<IAbstractMessage>(var message)
                || await requestStorage.TryGetAudit(serverOptions.InstanceId, index, token) is not Some<Audit>(var audit))
            {
                logger.LogDebug("#{Index:D5}: No message has found yet.", index);
                await Task.WhenAny(Task.Delay(serverOptions.InactivityDelayTime, token));
                continue;
            }

            var messageName = typeEncoder.Encode(message.GetType())
                              ?? throw new NotSupportedException($"Not supported  message type '{message.GetType()}'.");
            var messageId = message.GetSha1();

            logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) found.", index, messageName, messageId);

            await using var scope = scopeFactory.CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
            var scopedProvider = scope.ServiceProvider;

            var diagnosticContext = scopedProvider.GetRequiredService<DiagnosticContext>();
            diagnosticContext.CorrelationId = audit.CorrelationId;

            var client = scopedProvider.GetRequiredService<IMessagingClient>();

            logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) handling: begins.",
                index, messageName, messageId);


            try
            {
                await responseStorage.AddOrGet(message, async _ =>
                {
                    CachingResult result;
                    try
                    {
                        var response = await client.RequestObject(message, token);
                        result = CachingResult.OfValue((dynamic)response);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "#{Index:D5}: Message({MessageName}/{MessageId}) handling: request failed.",
                            index, messageName, messageId);
                        result = CachingResult.OfException(ex);
                    }

                    logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) handling: request responded.",
                        index, messageName, messageId);
                    return result;
                }, token);
            }
            catch (OperationCanceledException ex) when (token.IsCancellationRequested)
            {
                logger.LogInformation(ex, "#{Index:D5}: Message({MessageName}/{MessageId}) handling: response storing cancelled.",
                    index, messageName, messageId);
                break;
            }

            logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) handling: succeeded.",
                index, messageName, messageId);

            try
            {
                await processedIndexStorage.AddOrUpdate(serverOptions.InstanceId, index, token);
            }
            catch (OperationCanceledException ex) when (token.IsCancellationRequested)
            {
                logger.LogInformation(ex, "#{Index:D5}: Message({MessageName}/{MessageId}) handling: index persisting cancelled.",
                    index, messageName, messageId);
                break;
            }

            logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) handling: index persisted.",
                index, messageName, messageId);

            index++;
            await Task.WhenAny(Task.Delay(serverOptions.NextMessageDelayTime, token));
        }

        logger.LogInformation("#{Index:D5}: Exit by cancellation.", index);
    }
}
