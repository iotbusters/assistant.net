using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
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
internal sealed class GenericMessageHandlingService : BackgroundService
{
    private readonly ILogger<GenericMessageHandlingService> logger;
    private readonly IOptionsMonitor<GenericHandlingServerOptions> options;
    private readonly ITypeEncoder typeEncoder;
    private readonly ISystemLifetime lifetime;
    private readonly IDisposable disposable;
    private readonly IPartitionedAdminStorage<int, IAbstractMessage> requestStorage;
    private readonly IAdminStorage<string, CachingResult> responseStorage;
    private readonly IStorage<int, long> processedIndexStorage;
    private readonly IServiceScopeFactory scopeFactory;

    public GenericMessageHandlingService(
        ILogger<GenericMessageHandlingService> logger,
        IOptionsMonitor<GenericHandlingServerOptions> options,
        ITypeEncoder typeEncoder,
        ISystemLifetime lifetime,
        IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.options = options;
        this.typeEncoder = typeEncoder;
        this.lifetime = lifetime;
        this.scopeFactory = scopeFactory;

        var scope = scopeFactory.CreateScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        disposable = scope;
        requestStorage = scope.ServiceProvider.GetRequiredService<IPartitionedAdminStorage<int, IAbstractMessage>>();
        responseStorage = scope.ServiceProvider.GetRequiredService<IAdminStorage<string, CachingResult>>();
        processedIndexStorage = scope.ServiceProvider.GetRequiredService<IStorage<int, long>>();
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        logger.LogDebug("Find processing index.");
        var index = await processedIndexStorage.GetOrDefault(options.CurrentValue.InstanceId, token) + 1;
        logger.LogInformation("{Index:D5} processing: begins.", index);

        var stoppingToken = lifetime.Stopping;
        while (!stoppingToken.IsCancellationRequested)
        {
            var serverOptions = options.CurrentValue;

            if (!await TryHandle(index, token))
            {
                logger.LogDebug("#{Index:D5} processing: message not found.", index);
                await Task.WhenAny(Task.Delay(serverOptions.InactivityDelayTime, token));
                continue;
            }

            logger.LogDebug("#{Index:D5} processing: index update begins.", index);

            try
            {
                await processedIndexStorage.AddOrUpdate(serverOptions.InstanceId, index, token);
            }
            catch (OperationCanceledException ex) when (token.IsCancellationRequested)
            {
                logger.LogInformation(ex, "#{Index:D5}: index update cancelled.", index);
                break;
            }

            logger.LogDebug("#{Index:D5} processing: index updated.", index);
            await Task.WhenAny(Task.Delay(serverOptions.NextMessageDelayTime, stoppingToken));
            index++;
        }

        logger.LogInformation("#{Index:D5} processing: cancelled.", index);
    }

    public override void Dispose()
    {
        disposable.Dispose();
        base.Dispose();
    }

    private async Task<bool> TryHandle(long index, CancellationToken token)
    {
        var serverOptions = options.CurrentValue;

        var requestOption = await requestStorage.TryGetDetailed(serverOptions.InstanceId, index, token);
        if (requestOption is not Some<PartitionValue<IAbstractMessage>>(var requestValue))
            return false;

        var messageId = requestValue.Value.GetSha1();
        var messageType = requestValue.Value.GetType();
        var messageName = typeEncoder.Encode(messageType);
        if (!serverOptions.MessageTypes.Contains(messageType))
        {
            logger.LogInformation("#{Index:D5} processing: Message({MessageName}, {MessageId}) is unknown.",
                index, messageName, messageId);
            return true;
        }

        logger.LogDebug("#{Index:D5} processing: Message({MessageName}, {MessageId}) found.", index, messageName, messageId);

        var requestId = requestValue[MessagePropertyNames.RequestIdName];
        if (requestId == null)
        {
            logger.LogCritical("#{Index:D5}: Message({MessageName}, {MessageId}, {RequestId}) processing: {PropertyName} is missing.",
                index, messageName, messageId, requestId, MessagePropertyNames.RequestIdName);
            return true;
        }

        var response = await Handle(requestValue, token);
        var responseValue = new StorageValue<CachingResult>(response)
        {
            CorrelationId = requestValue.CorrelationId,
            User = requestValue.User
        };
        await responseStorage.AddOrGet(requestId, responseValue, token);

        logger.LogDebug("#{Index:D5}: Message({MessageName}, {MessageId}, {RequestId}) processing: responded.",
            index, messageName, messageId, requestId);
        return true;
    }

    private async Task<CachingResult> Handle(PartitionValue<IAbstractMessage> value, CancellationToken token)
    {
        var requestId = value.Details.GetOrDefault(MessagePropertyNames.RequestIdName);
        var message = value.Value;
        var messageName = typeEncoder.Encode(message.GetType());
        var messageId = message.GetSha1();

        await using var scope = scopeFactory
            .CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName)
            .ConfigureDiagnosticContext(value.CorrelationId, value.User);

        var client = scope.ServiceProvider.GetRequiredService<IMessagingClient>();

        logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) handling: begins.",
            messageName, messageId, requestId);

        object response;
        try
        {
            response = await client.RequestObject(message, token);
        }
        catch (OperationCanceledException ex) when (!token.IsCancellationRequested)
        {
            logger.LogWarning("Message({MessageName}, {MessageId}, {RequestId}) handling: cancelled.",
                messageName, messageId, requestId);

            return CachingResult.OfException(new MessageDeferredException("Message handling was cancelled.", ex));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message({MessageName}, {MessageId}, {RequestId}) handling: failed.",
                messageName, messageId, requestId);

            return CachingResult.OfException(ex);
        }

        logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) handling: succeeded.",
            messageName, messageId, requestId);

        return CachingResult.OfValue((dynamic)response);
    }
}
