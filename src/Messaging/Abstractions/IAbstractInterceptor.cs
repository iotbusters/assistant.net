using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     De-typed interceptor abstraction that helps intercepting unknown messages
///     during runtime without reflection related performance drop.
/// </summary>
public interface IAbstractInterceptor
{
    /// <summary>
    ///     Executes some logic before and after intercepted behaviour in <paramref name="next"/>.
    /// </summary>
    Task<object> Intercept(Func<IAbstractMessage, CancellationToken, Task<object>> next, IAbstractMessage message, CancellationToken token = default);
}
