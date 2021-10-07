using Assistant.Net.Messaging.Abstractions;
using System.Threading;
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
        public static Task<TResponse> Send<TResponse>(this IMessagingClient client, IMessage<TResponse> message, CancellationToken token = default) => client
            .SendObject(message, token).MapCompleted(x => (TResponse)x);

        /// <summary>
        ///     Sends asynchronously a request to associated request handler.
        ///     Similar to request although in opposite expecting successful execution only.
        /// </summary>
        public static Task Send(this IMessagingClient client, IMessage message, CancellationToken token = default) => client
            .Send<None>(message, token);
    }
}
