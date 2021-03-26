using System.Threading.Tasks;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface ICommandClient
    {
        /// <summary>
        ///     Sends asynchronously a request to associated request handler expecting a specific object in respond.
        /// </summary>
        /// <typeparam name="TResponse">Response object type.</typeparam>
        /// <exception cref="CommandTimeoutException" />
        /// <exception cref="CommandHandlerNotFoundException" />
        Task<TResponse> Send<TResponse>(ICommand<TResponse> command);

        /// <summary>
        ///     Sends asynchronously a request to associated request handler.
        ///     Similar to request although in opposite expecting successful execution only.
        /// </summary>
        /// <exception cref="CommandFailedException" />
        /// <exception cref="CommandTimeoutException" />
        /// <exception cref="CommandHandlerNotFoundException" />
        Task Send(ICommand command)
            => Send<None>(command);
    }
}