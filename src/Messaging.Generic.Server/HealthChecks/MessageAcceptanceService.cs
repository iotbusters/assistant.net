using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.HealthChecks;

/// <summary>
///     Message acceptance for remote handling registration service.
/// </summary>
public sealed class MessageAcceptanceService : IDisposable
{
    private readonly ILogger<MessageAcceptanceService> logger;
    private readonly IOptionsMonitor<GenericHandlingServerOptions> options;
    private readonly string instance;
    private readonly ITypeEncoder typeEncoder;
    private readonly ISystemClock clock;
    private readonly IServiceScope globalScope;
    private readonly IStorage<string, RemoteHandlerModel> remoteHostRegistrationStorage;

    private Type[] registeredMessageTypes;

    /// <summary/>
    public MessageAcceptanceService(
        ILogger<MessageAcceptanceService> logger,
        IOptionsMonitor<GenericHandlingServerOptions> options,
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ITypeEncoder typeEncoder,
        ISystemClock clock)
    {
        this.logger = logger;
        this.registeredMessageTypes = Array.Empty<Type>();
        this.options = options;
        this.instance = environment.ApplicationName;
        this.typeEncoder = typeEncoder;
        this.clock = clock;
        this.globalScope = scopeFactory.CreateScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        this.remoteHostRegistrationStorage = globalScope.ServiceProvider.GetRequiredService<IStorage<string, RemoteHandlerModel>>();
    }

    /// <inheritdoc/>
    void IDisposable.Dispose() => globalScope.Dispose();

    /// <summary>
    ///     Refresh accepting messages by the hosting server.
    /// </summary>
    /// <param name="timeToLive">The time to life before a registration is expired.</param>
    /// <param name="token"/>
    public async Task Register(TimeSpan timeToLive, CancellationToken token)
    {
        var now = clock.UtcNow;
        var expired = now.Add(timeToLive);

        var localRegisteredMessageTypes = registeredMessageTypes;
        var localLatestMessageTypes = options.CurrentValue.MessageTypes.ToArray();
        var unregisteredMessageTypes = localRegisteredMessageTypes.Except(localLatestMessageTypes);

        foreach (var messageType in localLatestMessageTypes)
        {
            var messageName = typeEncoder.Encode(messageType)!;
            await remoteHostRegistrationStorage.AddOrUpdate(
                key: messageName,
                addFactory: _ => new RemoteHandlerModel().AddInstance(instance, expired),
                updateFactory: (_, current) => current.AddInstance(instance, expired).Skip(expiredBefore: now),
                token);
            logger.LogWarning("Message({MessageName}) acceptance at {Instance}: updated.", messageName, instance);
        }

        foreach (var messageType in unregisteredMessageTypes)
        {
            var messageName = typeEncoder.Encode(messageType)!;
            await remoteHostRegistrationStorage.AddOrUpdate(
                key: messageName,
                addFactory: _ => new RemoteHandlerModel(),
                updateFactory: (_, current) => current.RemoveInstance(instance).Skip(expiredBefore: now),
                token);
            logger.LogWarning("Message({MessageName}) acceptance at {Instance}: unregistered.", messageName, instance);
        }

        registeredMessageTypes = localLatestMessageTypes;
    }

    /// <summary>
    ///     Stop accepting messages by the hosting server.
    /// </summary>
    public async Task Unregister(CancellationToken token)
    {
        var now = clock.UtcNow;
        var messageTypes = registeredMessageTypes.Concat(options.CurrentValue.MessageTypes).ToHashSet();

        foreach (var messageType in messageTypes)
        {
            var messageName = typeEncoder.Encode(messageType)!;
            await remoteHostRegistrationStorage.AddOrUpdate(
                key: messageName,
                addFactory: _ => new RemoteHandlerModel(),
                updateFactory: (_, current) => current.RemoveInstance(instance).Skip(expiredBefore: now),
                token);
            logger.LogWarning("Message({MessageName}) acceptance at {Instance}: unregister.", messageName, instance);
        }

        this.registeredMessageTypes = Array.Empty<Type>();
    }
}
