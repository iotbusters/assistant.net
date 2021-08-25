using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Message handling client abstraction.
    /// </summary>
    public interface IMessagingClient
    {
        /// <summary>
        ///     Sends a message asynchronously to associated message handler and expects an object in respond.
        /// </summary>
        Task<object> Send(object message);
    }
}