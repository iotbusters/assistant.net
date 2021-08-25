using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Message handler abstraction that accepts <typeparamref name="TMessage" /> only and returns <typeparamref name="TResponse" /> in response.
    /// </summary>
    /// <typeparam name="TMessage">Specific message implementation type.</typeparam>
    /// <typeparam name="TResponse">Response type of <typeparamref name="TMessage"/>.</typeparam>
    public interface IMessageHandler<in TMessage, TResponse> : IAbstractHandler
        where TMessage : IMessage<TResponse>
    {
        /// <summary>
        ///     Handles <typeparamref name="TMessage" /> object.
        /// </summary>
        Task<TResponse> Handle(TMessage message);

        Task<object> IAbstractHandler.Handle(object message) => Handle((TMessage) message).MapSuccess(x => (object) x!);
    }

    /// <summary>
    ///     Message handler abstraction that accepts <typeparamref name="TMessage" /> only when no object in response is expected.
    /// </summary>
    /// <typeparam name="TMessage">Specific message implementation type.</typeparam>
    public interface IMessageHandler<in TMessage> : IMessageHandler<TMessage, None>
        where TMessage : IMessage
    {
        /// <summary>
        ///     Handles <typeparamref name="TMessage" /> object.
        /// </summary>
        new Task Handle(TMessage message);

        async Task<None> IMessageHandler<TMessage, None>.Handle(TMessage message)
        {
            await Handle(message);
            return None.Instance;
        }
    }
}