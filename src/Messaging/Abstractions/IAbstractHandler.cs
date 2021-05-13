using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     De-typed command handler abstraction that helps handling unknown commands 
    ///     during runtime without reflection related performance drop.
    /// </summary>
    public interface IAbstractHandler
    {
        /// <summary>
        ///     Handles <paramref name="command" /> object.
        /// </summary>
        Task<object> Handle(object command);
    }
}