using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Remote message handling client abstraction.
    /// </summary>
    public interface IRemoteMessagingClient
    {
         /// <summary>
        ///     Delegates <paramref name="message"/> handling to remote handler.
        /// </summary>
        Task<TResponse> DelegateHandling<TResponse>(IMessage<TResponse> message);
    }
}