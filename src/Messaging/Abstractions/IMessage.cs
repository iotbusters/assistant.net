namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     Message abstraction that expects <typeparamref name="TResponse"/> object in response to the request.
/// </summary>
public interface IMessage<out TResponse> : IAbstractMessage { }

/// <summary>
///     Message abstraction that doesn't expect a response.
/// </summary>
public interface IMessage : IMessage<Nothing> { }
