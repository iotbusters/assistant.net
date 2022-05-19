using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
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
///     Message handling orchestrating service.
/// </summary>
internal class MessageHandlingService : BackgroundService
{
    private readonly ILogger<MessageHandlingService> logger;
    private readonly IOptionsMonitor<MongoHandlingServerOptions> options;
    private readonly IPartitionedAdminStorage<int, IAbstractMessage> requestStorage;
    private readonly IStorage<int, long> processedIndexStorage;
    private readonly ITypeEncoder typeEncoder;
    private readonly IServiceProvider provider;

    public MessageHandlingService(
        ILogger<MessageHandlingService> logger,
        IOptionsMonitor<MongoHandlingServerOptions> options,
        IPartitionedAdminStorage<int, IAbstractMessage> requestStorage,
        IStorage<int, long> processedIndexStorage,
        ITypeEncoder typeEncoder,
        IServiceProvider provider)
    {
        this.logger = logger;
        this.options = options;
        this.requestStorage = requestStorage;
        this.processedIndexStorage = processedIndexStorage;
        this.typeEncoder = typeEncoder;
        this.provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var index = 1L;
        while (!token.IsCancellationRequested)
        {
            logger.LogDebug("#{Index:D5}: Find next message.", index);

            var serverOptions = options.CurrentValue;
            if (await requestStorage.TryGet(serverOptions.InstanceId, index, token) is not Some<IAbstractMessage>(var message)
                || await requestStorage.TryGetAudit(serverOptions.InstanceId, index, token) is not Some<Storage.Models.Audit>(var audit))
            {
                logger.LogDebug("#{Index:D5}: No message has found yet.", index);
                await Task.Delay(serverOptions.InactivityDelayTime, token);
                continue;
            }

            var messageName = typeEncoder.Encode(message.GetType())
                              ?? throw new NotSupportedException($"Not supported  message type '{message.GetType()}'.");
            var messageId = message.GetSha1();

            logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) found.", index, messageName, messageId);

            await using var scope = provider.CreateAsyncScope();
            var scopedProvider = scope.ServiceProvider;

            var diagnosticContext = scopedProvider.GetRequiredService<DiagnosticContext>();
            diagnosticContext.CorrelationId = audit.CorrelationId;

            var clientFactory = scopedProvider.GetRequiredService<IMessagingClientFactory>();
            var client = clientFactory.Create(MongoOptionsNames.DefaultName);

            logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) publishing: begins.", index, messageName, messageId);

            try
            {
                await client.PublishObject(message, token);
            }
            catch (OperationCanceledException ex) when (token.IsCancellationRequested)
            {
                logger.LogInformation(ex, "#{Index:D5}: Message({MessageName}/{MessageId}) publishing: cancelled.", index, messageName, messageId);
                break;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "#{Index:D5}: Message({MessageName}/{MessageId}) publishing: failed.", index, messageName, messageId);
            }
            finally
            {
                logger.LogDebug("#{Index:D5}: Message({MessageName}/{MessageId}) publishing: ends.", index, messageName, messageId);
            }

            if (index % 100 == 0)
            {
                logger.LogDebug("#{Index:D5}: Old message cleanup: begins.", index);
                await processedIndexStorage.AddOrUpdate(serverOptions.InstanceId, index, token);
                await requestStorage.TryRemove(serverOptions.InstanceId, index, token);
                logger.LogDebug("#{Index:D5}: Old message Cleanup: ends.", index);
            }

            index++;
            await Task.Delay(serverOptions.NextMessageDelayTime, token);
        }

        logger.LogInformation("#{Index:D5}: Exit by cancellation.", index);
    }
}
