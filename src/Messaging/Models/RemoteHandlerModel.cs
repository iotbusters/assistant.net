using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Assistant.Net.Messaging.Models;

/// <summary>
///     Remote message handler configuration model.
/// </summary>
public class RemoteHandlerModel
{
    private static volatile int indexer;

    private readonly Dictionary<string, RemoteHandlerRegistration> registeredInstances;

    /// <summary/>
    public RemoteHandlerModel(params RemoteHandlerRegistration[] registeredInstances) =>
        this.registeredInstances = registeredInstances.ToDictionary(x => x.Instance);

    /// <summary>
    ///     List of registered remote message handlers.
    /// </summary>
    public RemoteHandlerRegistration[] RegisteredInstances => registeredInstances.Values.ToArray();

    /// <summary>
    ///     Determine if any registered remote message handlers.
    /// </summary>
    public bool HasRegistrations => registeredInstances.Any();

    /// <summary>
    ///     Registers new remote message handler.
    /// </summary>
    /// <param name="instance">Remote server instance name.</param>
    /// <param name="expired">The time when current record can be abandoned.</param>
    public RemoteHandlerModel AddInstance(string instance, DateTimeOffset expired)
    {
        registeredInstances[instance] = new(instance, expired);
        return this;
    }

    /// <summary>
    ///     Unregisters remote message handler.
    /// </summary>
    /// <param name="instance">Remote server instance name.</param>
    public RemoteHandlerModel RemoveInstance(string instance)
    {
        registeredInstances.Remove(instance);
        return this;
    }

    /// <summary>
    ///     Skips expired remote message handlers.
    /// </summary>
    /// <param name="expiredBefore">The time to compare remote message handler expiration against.</param>
    public RemoteHandlerModel Skip(DateTimeOffset expiredBefore)
    {
        var expiredInstances = registeredInstances.Keys.Where(instance => registeredInstances[instance].Expired < expiredBefore);
        foreach (var instance in expiredInstances)
            registeredInstances.Remove(instance);
        return this;
    }

    /// <summary>
    ///     Gets one of registered instance in round-robin manner, or null if no registrations.
    /// </summary>
    public string? GetInstance() => registeredInstances.Count switch
    {
        0     => null,
        1     => registeredInstances.Keys.Single(),
        var l => registeredInstances.Keys.ElementAt(Interlocked.Increment(ref indexer) % l)
    };
}
