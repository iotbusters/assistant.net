using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     De-typed message handler abstraction that helps handling unknown messages
    ///     during runtime without reflection related performance drop.
    /// </summary>
    public interface IAbstractHandler
    {
        /// <summary>
        ///     Requests the <paramref name="message" /> object handling to receive a response.
        /// </summary>
        /// <remarks>
        ///     Request-response behavior.
        /// </remarks>
        Task<object> Request(object message, CancellationToken token = default);

        /// <summary>
        ///     Publishes the <paramref name="message" /> object handling without waiting for a response.
        /// </summary>
        /// <remarks>
        ///     Fire-and-forget behavior.
        /// </remarks>
        Task Publish(object message, CancellationToken token);
    }
}
