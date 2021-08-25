namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Message abstraction that expects <typeparamref name="TResponse" /> object in response to the request.
    /// </summary>
    public interface IMessage<out TResponse> : IAbstractMessage
    {
    }

    /// <summary>
    ///     Message abstraction that doesn't expect 
    /// </summary>
    public interface IMessage : IMessage<None>
    {
    }

    /// <summary>
    ///     Very generic message abstraction that is used primarily for type restrictions 
    ///     in configuration and other internal logic.
    /// </summary>
    public interface IAbstractMessage
    {
    }

    /// <summary>
    ///     No message response representation.
    /// </summary>
    public sealed class None
    {
        internal static None Instance { get; } = new None();
    }
}