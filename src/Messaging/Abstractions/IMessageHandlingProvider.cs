namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     A specific data provider abstraction for message handling mechanism.
    /// </summary>
    /// <typeparam name="TMessage">Specific message implementation type.</typeparam>
    /// <typeparam name="TResponse">Response type of <typeparamref name="TMessage"/>.</typeparam>
    public interface IMessageHandlingProvider<TMessage, TResponse> : IAbstractHandler where TMessage : IMessage<TResponse> { }
}
