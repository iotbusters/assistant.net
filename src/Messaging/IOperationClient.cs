using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging
{
    public interface IOperationClient
    {
        /// <summary>
        ///     Sends asynchronously a request to associated request handler expecting a specific object in respond.
        /// </summary>
        /// <typeparam name="TResponse">Response object type.</typeparam>
        /// <exception cref="RequestTimeoutException" />
        Promise<TResponse> Send<TResponse>(IRequest<TResponse> request);

        /// <summary>
        ///     Sends asynchronously a request to associated request handler.
        ///     Similar to request although in opposite expecting successful execution only.
        /// </summary>
        /// <exception cref="RequestFailedException" />
        /// <exception cref="RequestTimeoutException" />
        Promise Send(IRequest request);
    }
}