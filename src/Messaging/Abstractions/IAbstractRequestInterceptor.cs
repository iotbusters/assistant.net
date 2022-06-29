using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     De-typed interceptor abstraction that helps intercepting requested unknown messages
///     during runtime without reflection related performance drop.
/// </summary>
public interface IAbstractRequestInterceptor
{
    /// <summary>
    ///     Executes custom behavior over <paramref name="next"/> message handler callback.
    /// </summary>
    ValueTask<object> Intercept(RequestMessageHandler next, IAbstractMessage message, CancellationToken token = default);
}

/// <summary>
///     A function requesting a message.
/// </summary>
public delegate ValueTask<object> RequestMessageHandler(IAbstractMessage message, CancellationToken token);
