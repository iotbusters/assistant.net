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
    private readonly ILogger logger;
    private readonly string instanceName;
    private readonly ITypeEncoder typeEncoder;
    private readonly ISystemLifetime lifetime;
    private readonly ISystemClock clock;
    private readonly IServiceScope globalScope;
    private readonly IPartitionedAdminStorage<string, IAbstractMessage> requestStorage;
    private readonly IAdminStorage<string, CachingResult> responseStorage;
    private readonly IStorage<string, long> processedIndexStorage;
    private readonly IServerActivityService activityService;
    private readonly IDisposable optionsRegistration;
    private readonly IDisposable loggerPropertyScope;

    private GenericHandlingServerOptions serverOptions;

    public GenericMessageHandlingService(
        string name,
        ILoggerFactory loggerFactory,
        IHostEnvironment environment,
        IOptionsMonitor<GenericHandlingServerOptions> options,
        ITypeEncoder typeEncoder,
        ISystemLifetime lifetime,
        ISystemClock clock,
        IServiceScopeFactory scopeFactory,
        IServerActivityService activityService)
    {
        this.logger = loggerFactory.CreateLogger(GetType().ToLoggerName(name));
        this.instanceName = InstanceName.Create(environment.ApplicationName, name);
        this.typeEncoder = typeEncoder;
        this.lifetime = lifetime;
        this.clock = clock;
        this.activityService = activityService;

        globalScope = scopeFactory.CreateScopeWithNamedOptionContext(name);
        requestStorage = globalScope.ServiceProvider.GetRequiredService<IPartitionedAdminStorage<string, IAbstractMessage>>();
        responseStorage = globalScope.ServiceProvider.GetRequiredService<IAdminStorage<string, CachingResult>>();
        processedIndexStorage = globalScope.ServiceProvider.GetRequiredService<IStorage<string, long>>();

        optionsRegistration = options.OnChange((o, n) =>
        {
            if (n == name)
                this.serverOptions = o;
        })!;
        this.serverOptions = options.Get(name);

        loggerPropertyScope = logger.BeginPropertyScope("InstanceName", instanceName);
    }

    public override void Dispose()
    {
        loggerPropertyScope.Dispose();
        optionsRegistration.Dispose();
        globalScope.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        logger.LogInformation("Message processing: begins.");

        var stoppingToken = lifetime.Stopping;

        var index = await GetLatestProcessedIndex(token) + 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            await activityService.DelayInactive(stoppingToken);

            if (!await TryHandle(index, token))
            {
                await Task.WhenAny(Task.Delay(serverOptions.InactivityDelayTime, stoppingToken));
                continue;
            }

            await StoreProcessedIndex(index, token);

            await Task.WhenAny(Task.Delay(serverOptions.NextMessageDelayTime, stoppingToken));
            index++;
        }

        logger.LogInformation("Message processing: ends.");
    }

    private async Task<long> GetLatestProcessedIndex(CancellationToken token)
    {
        logger.LogDebug("Processing index get: begins.");

        while (!token.IsCancellationRequested)
        {
            Option<long> option;
            try
            {
                option = await processedIndexStorage.TryGet(instanceName, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Processing index get: failed.");
                continue;
            }

            if (option is Some<long>(var index))
            {
                logger.LogInformation("Processing index get: ends with {Index}.", index);
                return index;
            }

            logger.LogInformation("Processing index get: ends without index.");
            return default; // initial value.
        }

        logger.LogWarning("Processing index get: cancelled.");
        return default; // safe exit: it won't be used.
    }

    private async Task StoreProcessedIndex(long index, CancellationToken token)
    {
        using var _ = logger.BeginPropertyScope("Index", index);

        logger.LogDebug("Processing index update: begins.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                await processedIndexStorage.AddOrUpdate(instanceName, index, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                continue;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Processing index update: failed.");
                continue;
            }

            logger.LogDebug("Processing index update: ends.");
            return;
        }

        logger.LogWarning("Processing index update: cancelled.");
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
        logger.LogDebug("Message polling: begins.");

        while (!token.IsCancellationRequested)
        {
            Option<PartitionValue<IAbstractMessage>> option;
            try
            {
                option = await requestStorage.TryGetDetailed(instanceName, index, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                continue;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Message polling: failed.");
                continue;
            }

            if (option is Some<PartitionValue<IAbstractMessage>>)
                logger.LogInformation("Message polling: ends with a message.");
            else
                logger.LogDebug("Message polling: ends without a message.");

            return option;
        }

        logger.LogWarning("Message polling: cancelled.");
        return Option.None;
    }

    private async Task<CachingResult> HandleMessage(PartitionValue<IAbstractMessage> messageValue, CancellationToken token)
    {
        var requestId = messageValue.Details.GetOrDefault(MessagePropertyNames.RequestIdName);
        var message = messageValue.Value;
        var messageId = message.GetSha1();
        var messageType = message.GetType();
        var messageName = typeEncoder.Encode(messageType);
        using var handlingLoggingScope = logger
            .BeginPropertyScope()
            .AddPropertyScope("CorrelationId", messageValue.CorrelationId!)
            .AddPropertyScope("User", messageValue.User!)
            .AddPropertyScope("MessageId", messageId);

        if (requestId == null)
        {
            logger.LogCritical("Message validation: property {PropertyName} is required.", MessagePropertyNames.RequestIdName);
            var exception = new MessageContractException($"Message({messageName ?? messageType.FullName}, {messageId}) "
                                                         + $"doesn't have required {MessagePropertyNames.RequestIdName}.");
            return CachingResult.OfException(exception);
        }

        handlingLoggingScope.AddPropertyScope("RequestId", requestId);

        if (messageName == null)
        {
            logger.LogCritical("Message validation: type {MessageType} isn't supported.", messageType.FullName);
            var exception = new MessageFailedException($"Requested({requestId}) Message({messageId}) has unsupported {messageType}.");
            return CachingResult.OfException(exception);
        }

        handlingLoggingScope.AddPropertyScope("MessageName", messageName);

        if (!serverOptions.HasBackoffHandler && !serverOptions.MessageTypes.Contains(messageType))
        {
            logger.LogWarning("Message validation: {MessageType} isn't registered on the server.", messageType.FullName);
            var exception = new MessageNotRegisteredException(
                $" Requested({requestId}) Message({messageName}, {messageId}) isn't registered on the server.");
            return CachingResult.OfException(exception);
        }

        var utcNow = clock.UtcNow;
        if (messageValue.Details.TryGetValue(MessagePropertyNames.ExpiredName, out var expiredString)
            && DateTimeOffset.TryParse(expiredString, out var expired)
            && expired <= utcNow)
        {
            var duration = utcNow - expired;
            logger.LogWarning("Message validation: already expired for {Duration}.", duration);
            var exception = new MessageFailedException(
                $"Requested({requestId}) Message({messageName}, {messageId}) is expired for {duration}.");
            return CachingResult.OfException(exception);
        }

        logger.LogInformation("Message processing: begins.");

        await using var handlingScope = globalScope.ServiceProvider
            .CloneAsyncScopeWithNamedOptionContext()
            .ConfigureDiagnosticContext(messageValue.CorrelationId, messageValue.User);
        var client = handlingScope.ServiceProvider.GetRequiredService<IMessagingClient>();

        object response;
        try
        {
            response = await client.RequestObject(message, token);
        }
        catch (OperationCanceledException ex) when (token.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Message processing: cancelled.");
            return default!; // safe exit: it won't be used.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Message processing: failed.");
            return CachingResult.OfException(ex);
        }

        logger.LogInformation("Message processing: ends.");
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

        logger.LogDebug("Message responding: begins.");

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
                logger.LogError(ex, "Message responding: failed.");
                continue;
            }

            logger.LogDebug("Message responding: ends.");
            return;
        }

        logger.LogWarning("Message responding: cancelled.");
    }
}
