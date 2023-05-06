using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace Assistant.Net.Messaging.HealthChecks;

internal sealed class ServerInstanceAvailabilityService : IDisposable
{
    private readonly IServiceScope scope;
    private readonly string instance;
    private readonly ILogger logger;
    private readonly IDisposable loggerScope;
    private readonly IAdminStorage<string, HostsAvailabilityModel> hostStorage;
    private readonly ISystemClock clock;
    private readonly ITypeEncoder typeEncoder;

    private (GenericHandlingServerOptions latest, GenericHandlingServerOptions previous) options;

    public ServerInstanceAvailabilityService(string name, IServiceScopeFactory scopeFactory)
    {
        scope = scopeFactory.CreateScopeWithNamedOptionContext(name);
        var provider = scope.ServiceProvider;
        var environment = provider.GetRequiredService<IHostEnvironment>();
        instance = InstanceName.Create(environment.ApplicationName, name);
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
        this.logger = loggerFactory.CreateLogger(GetType().ToLoggerName(name));
        this.loggerScope = logger.BeginPropertyScope("InstanceName", instance);
        this.hostStorage = provider.GetRequiredService<IAdminStorage<string, HostsAvailabilityModel>>();
        this.clock = provider.GetRequiredService<ISystemClock>();
        this.typeEncoder = provider.GetRequiredService<ITypeEncoder>();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<GenericHandlingServerOptions>>();
        this.options = (latest: optionsMonitor.Get(name), previous: new());
    }

    public void Dispose()
    {
        loggerScope.Dispose();
        scope.Dispose();
    }

    public void Change(GenericHandlingServerOptions changedOptions)
    {
        logger.LogDebug("Configuration reloading: begins.");

        this.options = (changedOptions, this.options.latest);

        logger.LogInformation("Configuration reloading: ends.");
    }

    public async Task Register(TimeSpan timeToLive, CancellationToken token)
    {
        logger.LogDebug("Message registration: begins.");

        var (latestOptions, previousOptions) = this.options;
        var messageNames = latestOptions.MessageTypes.Select(typeEncoder.Encode).ToArray();
        var acceptOthers = latestOptions.HasBackoffHandler;
        var now = clock.UtcNow;
        var expired = now.Add(timeToLive);

        var model = await hostStorage.AddOrUpdate(
            key: HostsAvailabilityModel.Key,
            addFactory: _ => new HostsAvailabilityModel()
                .Add(instance, messageNames!, acceptOthers, expired),
            updateFactory: (_, current) => current
                .Remove(expiredBefore: now)
                .Add(instance, messageNames!, acceptOthers, expired),
            token);

        foreach (var host in model.Registrations)
            logger.LogDebug("Message registration: updated {MessageNames} to be expired at {ExpiredTime:hh:mm:ss.fff}.",
                host.Messages, host.Expired);

        var registeredMessageNames = latestOptions.MessageTypes
            .Except(previousOptions.MessageTypes)
            .Select(typeEncoder.Encode);
        foreach (var messageName in registeredMessageNames)
            logger.LogInformation("Message registration: started accepting {MessageName}.", messageName);

        var unregisteredMessageNames = previousOptions.MessageTypes
            .Except(latestOptions.MessageTypes)
            .Select(typeEncoder.Encode);
        foreach (var messageName in unregisteredMessageNames)
            logger.LogWarning("Message registration: stopped accepting {MessageName}.", messageName);

        logger.LogDebug("Message registration: ends.");
    }

    public async Task Unregister(CancellationToken token)
    {
        logger.LogDebug("Message registration: begins.");

        var (latestOptions, previousOptions) = this.options;
        var now = clock.UtcNow;

        await hostStorage.AddOrUpdate(
            key: HostsAvailabilityModel.Key,
            addFactory: _ => new(),
            updateFactory: (_, current) => current.Remove(instance).Remove(expiredBefore: now),
            token);

        var unregisteredMessageNames = latestOptions.MessageTypes
            .Concat(previousOptions.MessageTypes)
            .Select(typeEncoder.Encode);
        foreach (var messageName in unregisteredMessageNames)
            logger.LogWarning("Message registration: stopped accepting {MessageName}.", messageName);

        logger.LogDebug("Message registration: ends.");
    }
}
