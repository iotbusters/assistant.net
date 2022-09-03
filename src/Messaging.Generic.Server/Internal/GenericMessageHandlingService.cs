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

        var index = await FindLatestProcessedIndex(stoppingToken) + 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            using var _ = logger.BeginPropertyScope("Index", index);
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
                await activityService.DelayInactive(token);
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

        logger.LogDebug("Update processing index: begins.");

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
                logger.LogError(ex, "Update processing index: failed.");
                await activityService.DelayInactive(token);
                continue;
            }

            logger.LogDebug("Update processing index: ends.");
            return;
        }

        logger.LogWarning("Update processing index: cancelled.");
    }

    private async Task<bool> TryHandle(long index, CancellationToken token)
    {
        var serverOptions = options.CurrentValue;

        if (await GetRequestedMessage(index, token) is not Some<PartitionValue<IAbstractMessage>>(var requestValue))
            return false;

        var requestId = requestValue.Details.GetOrDefault(MessagePropertyNames.RequestIdName);
        var messageId = requestValue.Value.GetSha1();
        var messageType = requestValue.Value.GetType();
        var messageName = typeEncoder.Encode(messageType);
        using var handlingScope = logger
            .BeginPropertyScope("RequestId", requestId!)
            .AddPropertyScope("MessageId", messageId)
            .AddPropertyScope("MessageName", messageName!);

        if (requestId == null)
        {
            logger.LogCritical("Process: message doesn't have required {PropertyName}.", MessagePropertyNames.RequestIdName);
            var exception = new MessageContractException($"Message({messageName ?? messageType.FullName}, {messageId}) "
                                                         + $"doesn't have required {MessagePropertyNames.RequestIdName}.");
            var failure = CachingResult.OfException(exception);
            await StoreMessageResponse(requestValue, failure, token);
            return true;
        }

        if (messageName == null)
        {
            logger.LogCritical("Process: unsupported {MessageType}.", messageType.FullName);
            var exception = new MessageFailedException($"Request({requestId})'s Message({messageId}) has unsupported {messageType}.");
            var failure = CachingResult.OfException(exception);
            await StoreMessageResponse(requestValue, failure, token);
            return true;
        }

        if (requestValue.Details.TryGetValue(MessagePropertyNames.ExpiredName, out var expiredString)
            && DateTimeOffset.TryParse(expiredString, out var expired)
            && expired <= clock.UtcNow)
        {
            logger.LogWarning("Process: request has expired.");
            var exception = new MessageFailedException($"Request({requestId})'s Message({messageName}, {messageId}) is expired.");
            var failure = CachingResult.OfException(exception);
            await StoreMessageResponse(requestValue, failure, token);
            return true;
        }

        if (!serverOptions.MessageTypes.Contains(messageType))
        {
            logger.LogWarning("Process: message type isn't registered on the server.");
            var exception = new MessageNotRegisteredException($" Request({requestId})'s Message({messageName}, {messageId}) isn't registered on the server.");
            var failure = CachingResult.OfException(exception);
            await StoreMessageResponse(requestValue, failure, token);
            return true;
        }

        logger.LogDebug("Process: message is accepted.");

        var response = await Handle(requestValue, token);
        await StoreMessageResponse(requestValue, response, token);
        return true;
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
                break;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Find requested message: failed.");
                await activityService.DelayInactive(token);
                continue;
            }

            logger.LogDebug("Find requested message: ends.");
            return option;
        }

        logger.LogWarning("Find requested message: cancelled.");
        return Option.None;
    }

    private async Task StoreMessageResponse(
        PartitionValue<IAbstractMessage> requestValue,
        CachingResult response,
        CancellationToken token)
    {
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
                break;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Respond: failed.");
                await activityService.DelayInactive(token);
                continue;
            }

            logger.LogDebug("Respond: ends.");
            return;
        }

        logger.LogWarning("Respond: cancelled.");
    }

    private async Task<CachingResult> Handle(PartitionValue<IAbstractMessage> value, CancellationToken token)
    {
        var message = value.Value;

        await using var scope = scopeFactory
            .CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName)
            .ConfigureDiagnosticContext(value.CorrelationId, value.User);
        var client = scope.ServiceProvider.GetRequiredService<IMessagingClient>();

        logger.LogDebug("Handle: begins.");

        object response;
        try
        {
            response = await client.RequestObject(message, token);
        }
        catch (OperationCanceledException ex) when (!token.IsCancellationRequested)
        {
            logger.LogWarning("Handle: cancelled.");
            return CachingResult.OfException(new MessageDeferredException("Message handling was cancelled.", ex));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Handle: failed.");
            return CachingResult.OfException(ex);
        }

        logger.LogInformation("Handle: ends.");
        return CachingResult.OfValue((dynamic)response);
    }
}
