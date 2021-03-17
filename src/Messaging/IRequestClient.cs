using System.Threading.Tasks;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging
{
    public interface IRequestClient
    {
        /// <summary>
        ///     Sends asynchronously a request to associated request handler expecting a specific object in respond.
        /// </summary>
        /// <typeparam name="TResponse">Response object type.</typeparam>
        /// <exception cref="RequestTimeoutException" />
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request);

        /// <summary>
        ///     Sends asynchronously a request to associated request handler.
        ///     Similar to request although in opposite expecting successful execution only.
        /// </summary>
        /// <exception cref="RequestFailedException" />
        /// <exception cref="RequestTimeoutException" />
        Task Send(IRequest request);
    }
}