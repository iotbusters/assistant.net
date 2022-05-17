using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Message handler abstraction that accepts <typeparamref name="TMessage" /> only and returns <typeparamref name="TResponse" /> in response.
/// </summary>
/// <typeparam name="TMessage">Specific message implementation type.</typeparam>
/// <typeparam name="TResponse">Response type of <typeparamref name="TMessage"/>.</typeparam>
public interface IMessageHandler<in TMessage, TResponse> where TMessage : IMessage<TResponse>
{
    /// <summary>
    ///     Handles <typeparamref name="TMessage" /> object.
    /// </summary>
    Task<TResponse> Handle(TMessage message, CancellationToken token = default);
}

/// <summary>
///     Message handler abstraction that accepts <typeparamref name="TMessage" /> only when no object in response is expected.
/// </summary>
/// <typeparam name="TMessage">Specific message implementation type.</typeparam>
public interface IMessageHandler<in TMessage> : IMessageHandler<TMessage, None> where TMessage : IMessage
{
    /// <summary>
    ///     Handles <typeparamref name="TMessage" /> object.
    /// </summary>
    new Task Handle(TMessage message, CancellationToken token = default);

    async Task<None> IMessageHandler<TMessage, None>.Handle(TMessage message, CancellationToken token)
    {
        await Handle(message, token);
        return None.Instance;
    }
}