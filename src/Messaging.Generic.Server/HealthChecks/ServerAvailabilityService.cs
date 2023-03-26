using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.HealthChecks;

/// <summary>
///     Server availability managing service required for remote message handling coordination.
/// </summary>
public sealed class ServerAvailabilityService : IDisposable
{
    private readonly SemaphoreSlim semaphore = new(initialCount: 1, maxCount: 1);
    private readonly ISet<string> registerMessageNames = new HashSet<string>();
    private readonly ISet<string> unregisterMessageNames = new HashSet<string>();

    private readonly ILogger<ServerAvailabilityService> logger;
    private readonly IDisposable disposable;
    private readonly string instanceName;
    private readonly ITypeEncoder typeEncoder;
    private readonly ISystemClock clock;
    private readonly IServiceScope globalScope;
    private readonly IStorage<string, RemoteHandlerModel> remoteHostRegistrationStorage;

    /// <summary/>
    public ServerAvailabilityService(
        ILogger<ServerAvailabilityService> logger,
        IOptionsMonitor<GenericHandlingServerOptions> options,
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ITypeEncoder typeEncoder,
        ISystemClock clock)
    {
        this.logger = logger;
        this.disposable = options.OnChange(ReloadMessageTypes)!;
        this.instanceName = environment.ApplicationName;
        this.typeEncoder = typeEncoder;
        this.clock = clock;
        this.globalScope = scopeFactory.CreateScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        this.remoteHostRegistrationStorage = globalScope.ServiceProvider.GetRequiredService<IStorage<string, RemoteHandlerModel>>();

        // note: the order matters!
        ReloadMessageTypes(options.CurrentValue);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        disposable.Dispose();
        globalScope.Dispose();
        semaphore.Dispose();
    }

    /// <summary>
    ///     Refresh accepting messages by the hosting server.
    /// </summary>
    /// <param name="timeToLive">The time to life before a registration is expired.</param>
    /// <param name="token"/>
    public async Task Register(TimeSpan timeToLive, CancellationToken token)
    {
        logger.LogDebug("Service {Instance} registration: begins.", instanceName);

        try
        {
            await semaphore.WaitAsync(token);

            var now = clock.UtcNow;
            var expired = now.Add(timeToLive);

            foreach (var messageName in registerMessageNames)
            {
                await remoteHostRegistrationStorage.AddOrUpdate(
                    key: messageName,
                    addFactory: _ => new RemoteHandlerModel().AddInstance(instanceName, expired),
                    updateFactory: (_, current) => current.AddInstance(instanceName, expired).Skip(expiredBefore: now),
                    token);
                logger.LogDebug("Message({MessageName}) acceptance at {Instance}: updated.", messageName, instanceName);
            }

            if (!unregisterMessageNames.Any())
                return;

            foreach (var messageName in unregisterMessageNames)
            {
                await remoteHostRegistrationStorage.AddOrUpdate(
                    key: messageName,
                    addFactory: _ => new RemoteHandlerModel(),
                    updateFactory: (_, current) => current.RemoveInstance(instanceName).Skip(expiredBefore: now),
                    token);
                logger.LogWarning("Message({MessageName}) acceptance at {Instance}: unregistered.", messageName, instanceName);
            }

            unregisterMessageNames.Clear();
        }
        finally
        {
            semaphore.Release();
            logger.LogDebug("Service {Instance} registration: ends.", instanceName);
        }
    }

    /// <summary>
    ///     Stop accepting messages by the hosting server.
    /// </summary>
    public async Task Unregister(CancellationToken token)
    {
        logger.LogDebug("Service {Instance} un-registration: begins.", instanceName);

        try
        {
            await semaphore.WaitAsync(token);

            var now = clock.UtcNow;

            foreach (var messageName in registerMessageNames.Concat(unregisterMessageNames))
            {
                await remoteHostRegistrationStorage.AddOrUpdate(
                    key: messageName,
                    addFactory: _ => new RemoteHandlerModel(),
                    updateFactory: (_, current) => current.RemoveInstance(instanceName).Skip(expiredBefore: now),
                    token);
                logger.LogWarning("Message({MessageName}) acceptance at {Instance}: unregister.", messageName, instanceName);
            }

            unregisterMessageNames.Clear();
        }
        finally
        {
            semaphore.Release();
            logger.LogDebug("Service {Instance} un-registration: ends.", instanceName);
        }
    }

    private void ReloadMessageTypes(GenericHandlingServerOptions options)
    {
        try
        {
            semaphore.Wait();

            var registeredMessageNames = registerMessageNames.ToArray();

            registerMessageNames.Clear();
            unregisterMessageNames.Clear();

            foreach (var messageType in options.MessageTypes)
            {
                var messageName = typeEncoder.Encode(messageType)!;
                registerMessageNames.Add(messageName);
            }

            foreach (var messageName in registeredMessageNames.Except(registerMessageNames))
                unregisterMessageNames.Add(messageName);
        }
        finally
        {
            semaphore.Release();
        }
    }

}
