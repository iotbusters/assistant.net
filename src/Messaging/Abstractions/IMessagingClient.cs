using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Message handling client abstraction.
/// </summary>
public interface IMessagingClient
{
    /// <summary>
    ///     Sends a message object asynchronously to associated message handler and expects an object in respond.
    /// </summary>
    /// <remarks>
    ///     Request-response behavior.
    /// </remarks>
    Task<object> RequestObject(IAbstractMessage message, CancellationToken token = default);

    /// <summary>
    ///     Sends a message object asynchronously to associated message handler without waiting for a response.
    /// </summary>
    /// <remarks>
    ///     Fire-and-forget behavior.
    /// </remarks>
    Task PublishObject(IAbstractMessage message, CancellationToken token = default);
}
