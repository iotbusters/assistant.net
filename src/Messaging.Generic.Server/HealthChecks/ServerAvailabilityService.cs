using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.HealthChecks;

/// <summary>
///     Message handling host availability registering mechanism implementation.
/// </summary>
internal sealed class ServerAvailabilityService : IServerAvailabilityService, IDisposable
{
    private readonly SemaphoreSlim semaphore = new(initialCount: 1, maxCount: 1);
    private readonly ConcurrentDictionary<string, ServerInstanceAvailabilityService> services = new();

    private readonly IServiceScopeFactory scopeFactory;
    private readonly IDisposable disposable;

    /// <summary/>
    public ServerAvailabilityService(
        IOptionsMonitor<GenericHandlingServerOptions> optionsMonitor,
        IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
        this.disposable = optionsMonitor.OnChange((options, name) =>
        {
            if (services.TryGetValue(name ?? Microsoft.Extensions.Options.Options.DefaultName, out var service))
                service.Change(options);
        })!;
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        disposable.Dispose();
        foreach (var service in services.Values)
            service.Dispose();
        services.Clear();
        semaphore.Dispose();
    }

    /// <inheritdoc cref="IServerAvailabilityService"/>
    public async Task Register(string name, TimeSpan timeToLive, CancellationToken token)
    {
        var service = services.GetOrAdd(name, _ => new(name, scopeFactory));
        await service.Register(timeToLive, token);
    }

    /// <inheritdoc cref="IServerAvailabilityService"/>
    public async Task Unregister(string name, CancellationToken token)
    {
        if (services.TryGetValue(name, out var service))
            await service.Unregister(token);
    }
}
