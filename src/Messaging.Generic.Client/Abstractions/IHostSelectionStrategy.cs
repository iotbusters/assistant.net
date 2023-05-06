using System.Threading.Tasks;
using System.Threading;
using System;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Registered message handling host selection strategy.
/// </summary>
public interface IHostSelectionStrategy
{
    /// <summary>
    ///     Gets one of registered hosts accepting <paramref name="messageType"/>.
    /// </summary>
    Task<string?> GetInstance(Type messageType, CancellationToken token);
}
