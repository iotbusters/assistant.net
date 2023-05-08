using Assistant.Net.Messaging.Abstractions;
using System;
using System.Linq;

namespace Assistant.Net.Messaging;

/// <summary>
///     System.Type extensions for message handling.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Makes a generic type from the definition <paramref name="genericTypeDefinition"/>
    ///     and its type parameters resolved from <paramref name="messageType"/>
    /// </summary>
    /// <param name="genericTypeDefinition">Generic type definition that requires two parameters: message type and message response type.</param>
    /// <param name="messageType">Specific message type.</param>
    /// <exception cref="ArgumentException"/>
    public static Type MakeGenericTypeBoundToMessage(this Type genericTypeDefinition, Type messageType)
    {
        if (!genericTypeDefinition.IsGenericTypeDefinition)
            throw new ArgumentException("Invalid generic type definition.", nameof(genericTypeDefinition));

        var responseType = messageType.GetResponseType()
                           ?? throw new ArgumentException("Invalid message type.", nameof(messageType));
        return genericTypeDefinition.MakeGenericType(messageType, responseType);
    }

    /// <summary>
    ///     Resolves message response type from message type.
    /// </summary>
    public static Type? GetResponseType(this Type messageType)
    {
        var abstractMessageType = messageType.GetInterfaces().SingleOrDefault(IsMessageInterface);
        if (abstractMessageType != null)
            return abstractMessageType.GetResponseType();

        if (messageType.IsMessageInterface())
            return messageType.GetGenericArguments().Single();

        return null;
    }

    /// <summary>
    ///     Gets all implemented the <see cref="IMessageHandler{TMessage,TResponse}"/> types.
    /// </summary>
    public static Type[] GetMessageHandlerInterfaceTypes(this Type handlerType)
    {
        var abstractHandlerTypes = handlerType.GetInterfaces().Where(x => x.IsMessageHandlerInterface()).ToArray();
        if (abstractHandlerTypes.Any())
            return abstractHandlerTypes;

        if (handlerType.IsMessageHandlerInterface())
            return new[] {handlerType};

        return Array.Empty<Type>();
    }

    /// <summary>
    ///     Verifies if provided <paramref name="messageType"/> implements the <see cref="IMessage{TResponse}"/>.
    /// </summary>
    public static bool IsMessage(this Type messageType) =>
        messageType.GetInterfaces().Any(x => x.IsMessageInterface());

    /// <summary>
    ///     Verifies if provided <paramref name="handlerType"/> implements the <see cref="IMessageHandler{TMessage,TResponse}"/>.
    /// </summary>
    public static bool IsMessageHandler(this Type handlerType) =>
        handlerType.GetInterfaces().Any(x => x.IsMessageHandlerInterface());

    /// <summary>
    ///     Verifies if provided <paramref name="messageType"/> is the <see cref="IMessage{TResponse}"/>.
    /// </summary>
    public static bool IsMessageInterface(this Type messageType) =>
        messageType is {IsInterface: true, IsGenericType: true} && messageType.GetGenericTypeDefinition() == typeof(IMessage<>);

    /// <summary>
    ///     Verifies if provided <paramref name="handlerType"/> is the <see cref="IMessageHandler{TMessage,TResponse}"/>.
    /// </summary>
    private static bool IsMessageHandlerInterface(this Type handlerType) =>
        handlerType is {IsInterface: true, IsGenericType: true} && handlerType.GetGenericTypeDefinition() == typeof(IMessageHandler<,>);

    /// <summary>
    ///     Gets all implemented the <see cref="IMessageRequestInterceptor{TMessage,TResponse}"/> types.
    /// </summary>
    public static Type[] GetMessageRequestInterceptorInterfaceTypes(this Type interceptorType)
    {
        var interceptorInterfaceTypes = interceptorType.GetInterfaces().Where(x => x.IsMessageRequestInterceptorInterface()).ToArray();
        if (interceptorInterfaceTypes.Any())
            return interceptorInterfaceTypes;

        if (interceptorType.IsMessageRequestInterceptorInterface())
            return new[] {interceptorType};

        return Array.Empty<Type>();
    }

    /// <summary>
    ///     Gets all implemented the <see cref="IMessagePublishInterceptor{TMessage}"/> types.
    /// </summary>
    public static Type[] GetMessagePublishInterceptorInterfaceTypes(this Type interceptorType)
    {
        var interceptorInterfaceTypes = interceptorType.GetInterfaces().Where(x => x.IsMessagePublishInterceptorInterface()).ToArray();
        if (interceptorInterfaceTypes.Any())
            return interceptorInterfaceTypes;

        if (interceptorType.IsMessagePublishInterceptorInterface())
            return new[] {interceptorType};

        return Array.Empty<Type>();
    }

    /// <summary>
    ///     Verifies if provided <paramref name="interceptorType"/> implements the <see cref="IMessageRequestInterceptor{TMessage,TResponse}"/>.
    /// </summary>
    public static bool IsRequestMessageInterceptor(this Type interceptorType) =>
        interceptorType.GetInterfaces().Any(x => x.IsMessageRequestInterceptorInterface());

    /// <summary>
    ///     Verifies if provided <paramref name="interceptorType"/> implements the <see cref="IMessagePublishInterceptor{TMessage}"/>.
    /// </summary>
    public static bool IsPublishMessageInterceptor(this Type interceptorType) =>
        interceptorType.GetInterfaces().Any(x => x.IsMessagePublishInterceptorInterface());

    /// <summary>
    ///     Verifies if provided <paramref name="interceptorType"/> implements the <see cref="IAbstractRequestInterceptor"/>.
    /// </summary>
    public static bool IsAbstractRequestInterceptor(this Type interceptorType) =>
        interceptorType.GetInterfaces().Any(x => x.IsAssignableTo(typeof(IAbstractRequestInterceptor)));

    /// <summary>
    ///     Verifies if provided <paramref name="interceptorType"/> implements the <see cref="IAbstractPublishInterceptor"/>.
    /// </summary>
    public static bool IsAbstractPublishInterceptor(this Type interceptorType) =>
        interceptorType.GetInterfaces().Any(x => x.IsAssignableTo(typeof(IAbstractPublishInterceptor)));

    /// <summary>
    ///     Verifies if provided <paramref name="interceptorType"/> is a message handler interface.
    /// </summary>
    private static bool IsMessageRequestInterceptorInterface(this Type interceptorType) =>
        interceptorType.IsInterface && interceptorType.IsGenericType && interceptorType.GetGenericTypeDefinition() == typeof(IMessageRequestInterceptor<,>);

    /// <summary>
    ///     Verifies if provided <paramref name="interceptorType"/> is a message handler interface.
    /// </summary>
    private static bool IsMessagePublishInterceptorInterface(this Type interceptorType) =>
        interceptorType.IsInterface && interceptorType.IsGenericType && interceptorType.GetGenericTypeDefinition() == typeof(IMessagePublishInterceptor<>);
}
