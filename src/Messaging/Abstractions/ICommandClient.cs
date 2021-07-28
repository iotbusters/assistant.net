using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface ICommandClient
    {
        /// <summary>
        ///     Sends asynchronously a request to associated request handler expecting a specific object in respond.
        /// </summary>
        Task<object> Send(object command);
    }
}