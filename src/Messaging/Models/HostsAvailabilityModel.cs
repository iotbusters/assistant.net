using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Models;

/// <summary>
///     TODO: Remote message handler configuration model.
/// </summary>
public class HostsAvailabilityModel
{
    /// <summary>
    ///     The key of a single handler registrations.
    /// </summary>
    public const string Key = "hosts";

    private readonly List<HostRegistrationModel> registrations;

    /// <summary/>
    public HostsAvailabilityModel(params HostRegistrationModel[] registrations) =>
        this.registrations = new(registrations);

    /// <summary>
    ///     List of registered remote message handlers.
    /// </summary>
    public HostRegistrationModel[] Registrations => registrations.ToArray();

    /// <summary>
    ///     Registers new remote message handler.
    /// </summary>
    /// <param name="instance">Remote server instance name.</param>
    /// <param name="messages"> TODO </param>
    /// <param name="acceptOthers"> TODO </param>
    /// <param name="expired">The time when current record can be abandoned.</param>
    public HostsAvailabilityModel Add(string instance, string[] messages, bool acceptOthers, DateTimeOffset expired)
    {
        var registration = new HostRegistrationModel(instance, messages, acceptOthers, expired);
        var index = registrations.FindIndex(x => x.Instance == instance);
        if (index >= 0)
            registrations[index] = registration;
        else
            registrations.Add(registration);
        return this;
    }

    /// <summary>
    ///     Unregisters remote message handler.
    /// </summary>
    /// <param name="instance">Remote server instance name.</param>
    public HostsAvailabilityModel Remove(string instance)
    {
        registrations.RemoveAll(x => x.Instance == instance);
        return this;
    }

    /// <summary>
    ///     Unregisters expired remote message handlers.
    /// </summary>
    /// <param name="expiredBefore">The time to compare remote message handler expiration against.</param>
    public HostsAvailabilityModel Remove(DateTimeOffset expiredBefore)
    {
        registrations.RemoveAll(x => x.Expired < expiredBefore);
        return this;
    }
}
