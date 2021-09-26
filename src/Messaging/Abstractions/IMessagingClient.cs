using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Message handling client abstraction.
    /// </summary>
    public interface IMessagingClient
    {
        /// <summary>
        ///     Sends a message object asynchronously to associated message handler and expects an object in respond.
        /// </summary>
        Task<object> SendObject(object message, CancellationToken token = default);
    }
}
