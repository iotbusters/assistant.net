using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Messaging client facilitating extensions.
    /// </summary>
    public static class MessagingClientExtensions
    {
        /// <summary>
        ///     Sends a request asynchronously to associated message handler and expects an object in respond.
        /// </summary>
        /// <remarks>
        ///     Request-response behavior.
        /// </remarks>
        public static async Task<TResponse> Request<TResponse>(this IMessagingClient client, IMessage<TResponse> message, CancellationToken token = default) =>
            (TResponse)await client.RequestObject(message, token);

        /// <summary>
        ///     Sends a request asynchronously to associated message handler expecting successful request handling only.
        /// </summary>
        /// <remarks>
        ///     Request-response behavior.
        /// </remarks>
        public static Task Request(this IMessagingClient client, IMessage message, CancellationToken token = default) => client
            .Request<None>(message, token);

        /// <summary>
        ///     Sends a message object asynchronously to associated message handler without waiting for a response.
        /// </summary>
        /// <remarks>
        ///     Fire-and-forget behavior.
        /// </remarks>
        public static Task Publish(this IMessagingClient client, IMessage message, CancellationToken token = default) => client
            .PublishObject(message, token);
    }
}
