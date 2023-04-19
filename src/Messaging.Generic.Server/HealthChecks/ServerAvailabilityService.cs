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
    private readonly Dictionary<string, IServiceScope> scopes = new();
    private readonly Dictionary<string, (ISet<string> register, ISet<string> unregister)> messageNames = new();

    private readonly ILogger<ServerAvailabilityService> logger;
    private readonly IOptionsMonitor<GenericHandlingServerOptions> optionsMonitor;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IDisposable disposable;
    private readonly string instanceName;
    private readonly ITypeEncoder typeEncoder;
    private readonly ISystemClock clock;

    /// <summary/>
    public ServerAvailabilityService(
        ILogger<ServerAvailabilityService> logger,
        IOptionsMonitor<GenericHandlingServerOptions> optionsMonitor,
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ITypeEncoder typeEncoder,
        ISystemClock clock)
    {
        this.logger = logger;
        this.optionsMonitor = optionsMonitor;
        this.scopeFactory = scopeFactory;
        this.disposable = optionsMonitor.OnChange(ReloadGenericHandlingServerOptions)!;
        this.instanceName = environment.ApplicationName;
        this.typeEncoder = typeEncoder;
        this.clock = clock;
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        disposable.Dispose();
        foreach (var scope in scopes.Values)
            scope.Dispose();
        semaphore.Dispose();
    }

    /// <summary>
    ///     Refresh accepting messages by the hosting server.
    /// </summary>
    /// <param name="name">The name of the storage options instance.</param>
    /// <param name="timeToLive">The time to life before a registration is expired.</param>
    /// <param name="token"/>
    public async Task Register(string name, TimeSpan timeToLive, CancellationToken token)
    {
        logger.LogDebug("Service {Instance} registration: begins.", instanceName);

        try
        {
            await semaphore.WaitAsync(token);

            if (!scopes.TryGetValue(name, out var scope) || !messageNames.TryGetValue(name, out var names))
            {
                scope = scopeFactory.CreateScopeWithNamedOptionContext(name);
                scopes.Add(name, scope);

                names = (new HashSet<string>(), new HashSet<string>());
                messageNames.Add(name, names);

                var options = optionsMonitor.Get(name);
                ReloadMessageTypes(options.MessageTypes, name);
            }

            var remoteHostRegistrationStorage = scope.ServiceProvider.GetRequiredService<IStorage<string, RemoteHandlerModel>>();
            var (registerMessageNames, unregisterMessageNames) = names;
            var now = clock.UtcNow;
            var expired = now.Add(timeToLive);

            foreach (var messageName in registerMessageNames)
            {
                await remoteHostRegistrationStorage.AddOrUpdate(
                    key: messageName,
                    addFactory: _ => new RemoteHandlerModel().AddInstance(instanceName, expired),
                    updateFactory: (_, current) => current.AddInstance(instanceName, expired).Skip(expiredBefore: now),
                    token);
                logger.LogDebug("Message({MessageName}) acceptance at {Instance}: updated for '{OptionName}'.",
                    messageName,
                    instanceName,
                    name);
            }

            if (!unregisterMessageNames.Any())
                return;

            foreach (var messageName in unregisterMessageNames)
            {
                await remoteHostRegistrationStorage.AddOrUpdate(
                    key: messageName,
                    addFactory: _ => new(),
                    updateFactory: (_, current) => current.RemoveInstance(instanceName).Skip(expiredBefore: now),
                    token);
                logger.LogWarning("Message({MessageName}) acceptance at {Instance}: unregistered for '{OptionName}'.",
                    messageName,
                    instanceName,
                    name);
            }

            unregisterMessageNames.Clear();
        }
        finally
        {
            semaphore.Release();
            logger.LogDebug("Service registration: ends.");
        }
    }

    /// <summary>
    ///     Stop accepting messages by the hosting server.
    /// </summary>
    /// <param name="name">The name of the storage options instance.</param>
    /// <param name="token"/>
    public async Task Unregister(string name, CancellationToken token)
    {
        logger.LogDebug("Service un-registration: begins.");

        try
        {
            await semaphore.WaitAsync(token);

            var now = clock.UtcNow;

            var remoteHostRegistrationStorage = scopes[name].ServiceProvider.GetRequiredService<IStorage<string, RemoteHandlerModel>>();
            var (registerMessageNames, unregisterMessageNames) = messageNames[name];

            foreach (var messageName in registerMessageNames.Concat(unregisterMessageNames))
            {
                await remoteHostRegistrationStorage.AddOrUpdate(
                    key: messageName,
                    addFactory: _ => new(),
                    updateFactory: (_, current) => current.RemoveInstance(instanceName).Skip(expiredBefore: now),
                    token);
                logger.LogWarning("Message({MessageName}) acceptance: unregister for '{OptionName}'.",
                    messageName,
                    name);
            }

            unregisterMessageNames.Clear();
        }
        finally
        {
            semaphore.Release();
            logger.LogDebug("Service un-registration: ends.");
        }
    }

    private void ReloadGenericHandlingServerOptions(GenericHandlingServerOptions options, string? name)
    {
        name ??= Microsoft.Extensions.Options.Options.DefaultName;

        try
        {
            semaphore.Wait();
            ReloadMessageTypes(options.MessageTypes, name);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private void ReloadMessageTypes(IEnumerable<Type> messageTypes, string name)
    {
        logger.LogDebug("Server configuration reloading: begins for '{OptionName}'.", name);

        var (registerMessageNames, unregisterMessageNames) = messageNames[name];
        var registeredMessageNames = registerMessageNames.ToArray();

        registerMessageNames.Clear();

        foreach (var messageType in messageTypes)
        {
            var messageName = typeEncoder.Encode(messageType)!;
            registerMessageNames.Add(messageName);
        }

        foreach (var messageName in registeredMessageNames.Except(registerMessageNames))
            unregisterMessageNames.Add(messageName);

        logger.LogDebug("Server configuration reloading: ends for '{OptionName}'.", name);
    }
}
