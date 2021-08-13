using Assistant.Net.Messaging.Abstractions;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging
{
    public static class CommandClientExtensions
    {
        /// <summary>
        ///     Sends asynchronously a request to associated request handler expecting a specific object in respond.
        /// </summary>
        /// <typeparam name="TResponse">Response object type.</typeparam>
        public static Task<TResponse> SendAs<TResponse>(this ICommandClient client, ICommand<TResponse> command) =>
            client.Send(command).MapSuccess(x => (TResponse)x);

        /// <summary>
        ///     Sends asynchronously a request to associated request handler.
        ///     Similar to request although in opposite expecting successful execution only.
        /// </summary>
        public static Task SendAs(this ICommandClient client, ICommand command) => client.SendAs<None>(command);
    }
}