using Assistant.Net.Messaging.Abstractions;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Messaging client facilitating extensions.
    /// </summary>
    public static class MessagingClientExtensions
    {
        /// <summary>
        ///     Sends asynchronously a request to associated request handler expecting a specific object in respond.
        /// </summary>
        /// <typeparam name="TResponse">Response object type.</typeparam>
        public static Task<TResponse> SendAs<TResponse>(this IMessagingClient client, IMessage<TResponse> message) =>
            client.Send(message).MapSuccess(x => (TResponse)x);

        /// <summary>
        ///     Sends asynchronously a request to associated request handler.
        ///     Similar to request although in opposite expecting successful execution only.
        /// </summary>
        public static Task SendAs(this IMessagingClient client, IMessage message) => client.SendAs<None>(message);
    }
}