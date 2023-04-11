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

        var index = await FindLatestProcessedIndex(token) + 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            await activityService.DelayInactive(stoppingToken);

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

        logger.LogDebug("Find processing index: begins.");

        while (!token.IsCancellationRequested)
        {
            long index;
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
                logger.LogError(ex, "Find processing index: failed.");
                continue;
            }

            logger.LogDebug("Find processing index: ends.");
            return index;
        }

        logger.LogWarning("Find processing index: cancelled.");
        return default; // safe exit: it won't be used.
    }

    private async Task StoreProcessedIndex(long index, CancellationToken token)
    {
        var instance = environment.ApplicationName;

        logger.LogDebug("Update processing {Index}: begins.", index);

        while (!token.IsCancellationRequested)
        {
            try
            {
                await processedIndexStorage.AddOrUpdate(instance, index, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                continue;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Update processing {Index}: failed.", index);
                continue;
            }

            logger.LogDebug("Update processing {Index}: ends.", index);
            return;
        }

        logger.LogWarning("Update processing {Index}: cancelled.", index);
    }

    private async Task<bool> TryHandle(long index, CancellationToken token)
    {
        if (await GetRequestedMessage(index, token) is not Some<PartitionValue<IAbstractMessage>>(var messageValue))
            return false;

        var response = await HandleMessage(messageValue, token);
        await StoreMessageResponse(messageValue, response, token);

        return !token.IsCancellationRequested;
    }

    private async Task<Option<PartitionValue<IAbstractMessage>>> GetRequestedMessage(long index, CancellationToken token)
    {
        var instance = environment.ApplicationName;

        logger.LogDebug("Find requested message: begins.");

        while (!token.IsCancellationRequested)
        {
            Option<PartitionValue<IAbstractMessage>> option;
            try
            {
                option = await requestStorage.TryGetDetailed(instance, index, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                continue;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Find requested message: failed.");
                continue;
            }

            logger.LogDebug("Find requested message: ends.");
            return option;
        }

        logger.LogWarning("Find requested message: cancelled.");
        return Option.None;
    }

    private async Task<CachingResult> HandleMessage(PartitionValue<IAbstractMessage> messageValue, CancellationToken token)
    {
        var serverOptions = options.CurrentValue;

        var requestId = messageValue.Details.GetOrDefault(MessagePropertyNames.RequestIdName);
        var messageId = messageValue.Value.GetSha1();
        var messageType = messageValue.Value.GetType();
        var messageName = typeEncoder.Encode(messageType);
        using var handlingLoggingScope = logger
            .BeginPropertyScope()
            .AddPropertyScope("CorrelationId", messageValue.CorrelationId!)
            .AddPropertyScope("User", messageValue.User!)
            .AddPropertyScope("RequestId", requestId!)
            .AddPropertyScope("MessageId", messageId)
            .AddPropertyScope("MessageName", messageName!);

        logger.LogDebug("Process: begins.");

        if (requestId == null)
        {
            logger.LogCritical("Process: message doesn't have required {PropertyName}.", MessagePropertyNames.RequestIdName);
            var exception = new MessageContractException($"Message({messageName ?? messageType.FullName}, {messageId}) "
                                                         + $"doesn't have required {MessagePropertyNames.RequestIdName}.");
            return CachingResult.OfException(exception);
        }

        if (messageName == null)
        {
            logger.LogCritical("Process: {MessageType} isn't supported.", messageType.FullName);
            var exception = new MessageFailedException($"Request({requestId})'s Message({messageId}) has unsupported {messageType}.");
            return CachingResult.OfException(exception);
        }

        if (!serverOptions.MessageTypes.Contains(messageType))
        {
            logger.LogWarning("Process: {MessageType} isn't registered on the server.", messageType.FullName);
            var exception = new MessageNotRegisteredException(
                $" Request({requestId})'s Message({messageName}, {messageId}) isn't registered on the server.");
            return CachingResult.OfException(exception);
        }

        var utcNow = clock.UtcNow;
        if (messageValue.Details.TryGetValue(MessagePropertyNames.ExpiredName, out var expiredString)
            && DateTimeOffset.TryParse(expiredString, out var expired)
            && expired <= utcNow)
        {
            var duration = utcNow - expired;
            logger.LogWarning("Process: message is expired for {Duration}.", duration);
            var exception = new MessageFailedException(
                $"Request({requestId})'s Message({messageName}, {messageId}) is expired for {duration}.");
            return CachingResult.OfException(exception);
        }

        logger.LogDebug("Process: message is accepted.");

        await using var handlingScope = scopeFactory
            .CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName)
            .ConfigureDiagnosticContext(messageValue.CorrelationId, messageValue.User);
        var client = handlingScope.ServiceProvider.GetRequiredService<IMessagingClient>();

        object response;
        try
        {
            response = await client.RequestObject(messageValue.Value, token);
        }
        catch (OperationCanceledException ex) when (token.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Process: cancelled.");
            return default!; // safe exit: it won't be used.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Process: failed.");
            return CachingResult.OfException(ex);
        }

        logger.LogInformation("Process: ends.");
        return CachingResult.OfValue((dynamic)response);
    }

    private async Task StoreMessageResponse(PartitionValue<IAbstractMessage> requestValue, CachingResult response, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return;

        var requestId = requestValue.Details.GetOrDefault(MessagePropertyNames.RequestIdName)!;
        var responseValue = new StorageValue<CachingResult>(response)
        {
            CorrelationId = requestValue.CorrelationId,
            User = requestValue.User
        };

        logger.LogDebug("Respond: begins.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                await responseStorage.AddOrGet(requestId, responseValue, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                continue;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Respond: failed.");
                continue;
            }

            logger.LogDebug("Respond: ends.");
            return;
        }

        logger.LogWarning("Respond: cancelled.");
    }
}
