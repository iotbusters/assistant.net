using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
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
internal sealed class GenericMessageHandlingService : BackgroundService
{
    private readonly ILogger<GenericMessageHandlingService> logger;
    private readonly IOptionsMonitor<GenericHandlingServerOptions> options;
    private readonly ITypeEncoder typeEncoder;
    private readonly IDisposable disposable;
    private readonly IPartitionedAdminStorage<int, IAbstractMessage> requestStorage;
    private readonly IStorage<int, long> processedIndexStorage;
    private readonly MessageHandler messageHandler;

    public GenericMessageHandlingService(
        ILogger<GenericMessageHandlingService> logger,
        IOptionsMonitor<GenericHandlingServerOptions> options,
        ITypeEncoder typeEncoder,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.options = options;
        this.typeEncoder = typeEncoder;
        var scope = scopeFactory.CreateScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        disposable = scope;
        requestStorage = scope.ServiceProvider.GetRequiredService<IPartitionedAdminStorage<int, IAbstractMessage>>();
        processedIndexStorage = scope.ServiceProvider.GetRequiredService<IStorage<int, long>>();
        messageHandler = scope.ServiceProvider.GetRequiredService<MessageHandler>();
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        logger.LogDebug("Find processing index.");
        var index = await processedIndexStorage.GetOrDefault(options.CurrentValue.InstanceId, token) + 1;
        logger.LogDebug("Found processing {Index:D5}.", index);

        while (!token.IsCancellationRequested)
        {
            logger.LogDebug("#{Index:D5}: Find next message.", index);

            var serverOptions = options.CurrentValue;

            if (await requestStorage.TryGet(serverOptions.InstanceId, index, token) is not Some<IAbstractMessage>(var message)
                || await requestStorage.TryGetAudit(serverOptions.InstanceId, index, token) is not Some<Audit>(var audit))
            {
                logger.LogDebug("#{Index:D5}: No message has found yet.", index);
                await Task.WhenAny(Task.Delay(serverOptions.InactivityDelayTime, token));
                continue;
            }

            var messageId = message.GetSha1();
            var messageType = message.GetType();
            var messageName = typeEncoder.Encode(messageType);
            if (!serverOptions.MessageTypes.Contains(messageType))
                logger.LogInformation("#{Index:D5}: Message({MessageName}/{MessageId}) is unknown.", index, messageName, messageId);
            else
            {
                logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) found.", index, messageName, messageId);
                await messageHandler.Handle(message, audit, token);
            }

            try
            {
                await processedIndexStorage.AddOrUpdate(serverOptions.InstanceId, index, token);
            }
            catch (OperationCanceledException ex) when (token.IsCancellationRequested)
            {
                logger.LogInformation(ex, "#{Index:D5}: Message({MessageName}/{MessageId}) index: cancelled.", index, messageName, messageId);
                break;
            }

            logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) index: updated.", index, messageName, messageId);
            await Task.WhenAny(Task.Delay(serverOptions.NextMessageDelayTime, token));
            index++;
        }

        logger.LogInformation("#{Index:D5}: Exit by cancellation.", index);
    }

    public override void Dispose()
    {
        disposable.Dispose();
        base.Dispose();
    }
}
