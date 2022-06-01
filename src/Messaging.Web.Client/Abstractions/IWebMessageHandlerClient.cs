using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Remote message handling client abstraction.
/// </summary>
public interface IWebMessageHandlerClient
{
    /// <summary>
    ///     Delegates <paramref name="message"/> handling to remote handler.
    /// </summary>
    Task<object> DelegateHandling(object message, CancellationToken token = default);
}
