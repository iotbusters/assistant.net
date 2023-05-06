using System.Threading.Tasks;
using System.Threading;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Message handling activation mechanism abstraction.
/// </summary>
public interface IServerActivityService
{
    /// <summary>
    ///     Determines if activation was requested.
    /// </summary>
    bool IsActivationRequested { get; }

    /// <summary>
    ///     Requests server activation.
    /// </summary>
    /// <remarks>
    ///     It ignores activated server.
    /// </remarks>
    void Activate();

    /// <summary>
    ///     Requests server inactivation.
    /// </summary>
    /// <remarks>
    ///     It ignores inactivated server.
    /// </remarks>
    void Inactivate();

    /// <summary>
    ///     Delays until server is activated.
    /// </summary>
    /// <remarks>
    ///     It ignores activated server.
    /// </remarks>
    Task DelayInactive(CancellationToken token);
}
