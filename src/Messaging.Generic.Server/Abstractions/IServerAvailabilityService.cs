using System.Threading.Tasks;
using System.Threading;
using System;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Message handling host availability registering mechanism abstraction.
/// </summary>
public interface IServerAvailabilityService
{
    /// <summary>
    ///     Refresh accepting messages by the hosting server.
    /// </summary>
    /// <param name="name">The name of the storage options instance.</param>
    /// <param name="timeToLive">The time to life before a registration is expired.</param>
    /// <param name="token"/>
    Task Register(string name, TimeSpan timeToLive, CancellationToken token);

    /// <summary>
    ///     Stop accepting messages by the hosting server.
    /// </summary>
    /// <param name="name">The name of the storage options instance.</param>
    /// <param name="token"/>
    Task Unregister(string name, CancellationToken token);
}
