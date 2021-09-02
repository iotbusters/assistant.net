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
        ///     Handles <paramref name="message" /> object.
        /// </summary>
        Task<object> Handle(object message, CancellationToken token = default);
    }
}