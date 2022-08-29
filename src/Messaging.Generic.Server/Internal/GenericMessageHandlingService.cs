using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.HealthChecks;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
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
    private readonly IHostEnvironment environment;
    private readonly IOptionsMonitor<GenericHandlingServerOptions> options;
    private readonly ITypeEncoder typeEncoder;
    private readonly ISystemLifetime lifetime;
    private readonly ISystemClock clock;
    private readonly IServiceScope globalScope;
    private readonly IPartitionedAdminStorage<string, IAbstractMessage> requestStorage;
    private readonly IAdminStorage<string, CachingResult> responseStorage;
    private readonly IStorage<string, long> processedIndexStorage;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ServerActivityService activityService;

    public GenericMessageHandlingService(
        ILogger<GenericMessageHandlingService> logger,
        IHostEnvironment environment,
        IOptionsMonitor<GenericHandlingServerOptions> options,
        ITypeEncoder typeEncoder,
        ISystemLifetime lifetime,
        ISystemClock clock,
        IServiceScopeFactory scopeFactory,
        ServerActivityService activityService)
    {
        this.logger = logger;
        this.environment = environment;
        this.options = options;
        this.typeEncoder = typeEncoder;
        this.lifetime = lifetime;
        this.clock = clock;
        this.scopeFactory = scopeFactory;
        this.activityService = activityService;

        globalScope = scopeFactory.CreateScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        requestStorage = globalScope.ServiceProvider.GetRequiredService<IPartitionedAdminStorage<string, IAbstractMessage>>();
        responseStorage = globalScope.ServiceProvider.GetRequiredService<IAdminStorage<string, CachingResult>>();
        processedIndexStorage = globalScope.ServiceProvider.GetRequiredService<IStorage<string, long>>();
    }

    public override void Dispose()
    {
        base.Dispose();
        globalScope.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var stoppingToken = lifetime.Stopping;

        logger.LogInformation("Requested message processing was started.");

        var index = await FindLatestProcessedIndex(stoppingToken) + 1;

        while (!stoppingToken.IsCancellationRequested)
        {
            await activityService.DelayInactive(token);

            var serverOptions = options.CurrentValue;

            if (!await TryHandle(index, token))
            {
                await Task.WhenAny(Task.Delay(serverOptions.InactivityDelayTime, stoppingToken));
                continue;
            }

            await StoreProcessedIndex(index, token);

            await Task.WhenAny(Task.Delay(serverOptions.NextMessageDelayTime, stoppingToken));
            index++;
        }

        logger.LogInformation("Requested message processing was stopped.");
    }

    private async Task<long> FindLatestProcessedIndex(CancellationToken token)
    {
        var instance = environment.ApplicationName;

        logger.LogDebug("Find processing index.");

        long index = 0;
        while (!token.IsCancellationRequested)
        {
            try
            {
                index = await processedIndexStorage.GetOrDefault(instance, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "#{Index:D5} processing index finding: failed.", index);
                await activityService.DelayInactive(token);
                continue;
            }

            logger.LogDebug("{Index:D5} processing index finding: found.", index);
            return index;
        }

        logger.LogWarning("#{Index:D5} processing index finding: cancelled.", index);
        return default; // safe exit: it won't be used.
    }

    private async Task StoreProcessedIndex(long index, CancellationToken token)
    {
        var instance = environment.ApplicationName;

        logger.LogDebug("#{Index:D5} index update: begins.", index);

        while (!token.IsCancellationRequested)
        {
            try
            {
                await processedIndexStorage.AddOrUpdate(instance, index, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "#{Index:D5} index updating: failed.", index);
                await activityService.DelayInactive(token);
                continue;
            }

            logger.LogDebug("#{Index:D5} index update: ends.", index);
            return;
        }

        logger.LogWarning("#{Index:D5} index updating: cancelled.", index);
    }

    private async Task<bool> TryHandle(long index, CancellationToken token)
    {
        var serverOptions = options.CurrentValue;

        if (await GetRequestedMessage(index, token) is not Some<PartitionValue<IAbstractMessage>>(var requestValue))
            return false;

        var messageId = requestValue.Value.GetSha1();
        var messageType = requestValue.Value.GetType();
        var messageName = typeEncoder.Encode(messageType);
        if (messageName == null)
        {
            logger.LogCritical("#{Index:D5} processing: Message({MessageId}) has unsupported {MessageType}.",
                index, messageId, messageType.FullName);
            var exception = new MessageFailedException($"Message({messageId}) has unsupported {messageType}.");
            var failure = CachingResult.OfException(exception);
            await StoreMessageResponse(requestValue, failure, token);
            return true;
        }

        if (requestValue.Details.TryGetValue(MessagePropertyNames.ExpiredName, out var expiredString)
            && DateTimeOffset.TryParse(expiredString, out var expired)
            && expired <= clock.UtcNow)
        {
            logger.LogWarning("#{Index:D5} processing: Message({MessageName}, {MessageId}) request is expired.",
                index, messageName, messageId);
            var exception = new MessageFailedException($"Message({messageName}, {messageId}) request is expired.");
            var failure = CachingResult.OfException(exception);
            await StoreMessageResponse(requestValue, failure, token);
            return true;
        }

        if (!serverOptions.MessageTypes.Contains(messageType))
        {
            logger.LogWarning("#{Index:D5} processing: Message({MessageName}, {MessageId}) isn't registered on the server.",
                index, messageName, messageId);
            var exception = new MessageNotRegisteredException($"Message({messageName}, {messageId}) isn't registered on the server.");
            var failure = CachingResult.OfException(exception);
            await StoreMessageResponse(requestValue, failure, token);
            return true;
        }

        logger.LogDebug("#{Index:D5} processing: Message({MessageName}, {MessageId}) found.", index, messageName, messageId);

        var requestId = requestValue.Details.GetOrDefault(MessagePropertyNames.RequestIdName);
        if (requestId == null)
        {
            logger.LogCritical("#{Index:D5} processing: Message({MessageName}, {MessageId}) doesn't have required {PropertyName}.",
                index, messageName, messageId, MessagePropertyNames.RequestIdName);
            var failure = CachingResult.OfException(new MessageContractException(
                $"Message({messageName}, {messageId}) doesn't have required {MessagePropertyNames.RequestIdName}."));
            await StoreMessageResponse(requestValue, failure, token);
            return true;
        }

        var response = await Handle(requestValue, token);
        await StoreMessageResponse(requestValue, response, token);
        return true;
    }

    private async Task<Option<PartitionValue<IAbstractMessage>>> GetRequestedMessage(long index, CancellationToken token)
    {
        var instance = environment.ApplicationName;

        logger.LogDebug("#{Index:D5} requested message finding: begins.", index);

        while (!token.IsCancellationRequested)
        {
            Option<PartitionValue<IAbstractMessage>> option;
            try
            {
                option = await requestStorage.TryGetDetailed(instance, index, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "#{Index:D5} requested message finding: failed.", index);
                await activityService.DelayInactive(token);
                continue;
            }

            logger.LogDebug("#{Index:D5} requested message finding: ends.", index);
            return option;
        }

        logger.LogWarning("#{Index:D5} requested message finding: cancelled.", index);
        return Option.None;
    }

    private async Task StoreMessageResponse(
        PartitionValue<IAbstractMessage> requestValue,
        CachingResult response,
        CancellationToken token)
    {
        var requestId = requestValue.Details.GetOrDefault(MessagePropertyNames.RequestIdName)!;
        var messageId = requestValue.Value.GetSha1();
        var messageType = requestValue.Value.GetType();
        var messageName = typeEncoder.Encode(messageType);
        var responseValue = new StorageValue<CachingResult>(response)
        {
            CorrelationId = requestValue.CorrelationId,
            User = requestValue.User
        };

        logger.LogDebug("Message({MessageName}, {MessageId}, {RequestId}) responding: begins.",
            messageName, messageId, requestId);

        while (!token.IsCancellationRequested)
        {
            try
            {
                await responseStorage.AddOrGet(requestId, responseValue, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Message({MessageName}, {MessageId}, {RequestId}) responding: failed.",
                    messageName, messageId, requestId);
                await activityService.DelayInactive(token);
                continue;
            }

            logger.LogDebug("Message({MessageName}, {MessageId}, {RequestId}) responding: ends.",
                messageName, messageId, requestId);
            return;
        }

        logger.LogWarning("Message({MessageName}, {MessageId}, {RequestId}) responding: cancelled.",
            messageName, messageId, requestId);
    }

    private async Task<CachingResult> Handle(PartitionValue<IAbstractMessage> value, CancellationToken token)
    {
        var requestId = value.Details.GetOrDefault(MessagePropertyNames.RequestIdName)!;
        var message = value.Value;
        var messageName = typeEncoder.Encode(message.GetType());
        var messageId = message.GetSha1();

        await using var scope = scopeFactory
            .CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName)
            .ConfigureDiagnosticContext(value.CorrelationId, value.User);

        var client = scope.ServiceProvider.GetRequiredService<IMessagingClient>();

        logger.LogDebug("Message({MessageName}, {MessageId}, {RequestId}) handling: begins.",
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

        logger.LogInformation("Message({MessageName}, {MessageId}, {RequestId}) handling: ends.",
            messageName, messageId, requestId);
        return CachingResult.OfValue((dynamic)response);
    }
}
