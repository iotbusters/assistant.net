using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     De-typed interceptor abstraction that helps intercepting published unknown messages
///     during runtime without reflection related performance drop.
/// </summary>
public interface IAbstractPublishInterceptor
{
    /// <summary>
    ///     Executes some logic before and after intercepted behaviour in <paramref name="next"/>.
    /// </summary>
    ValueTask Intercept(PublishMessageHandler next, IAbstractMessage message, CancellationToken token = default);
}

/// <summary>
///     A function publishing a message.
/// </summary>
public delegate ValueTask PublishMessageHandler(IAbstractMessage message, CancellationToken token);
