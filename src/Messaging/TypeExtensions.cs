using Assistant.Net.Messaging.Abstractions;
using System;
using System.Linq;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     System.Type extensions for messaging handling.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Makes a generic type from the definition <paramref name="genericTypeDefinition" />
        ///     and its type parameters resolved from <paramref name="messageType" />
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
        ///     Gets all implemented message handler interface types from message handler type.
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
        ///     Verifies if provided <paramref name="messageType" /> implements a message interface.
        /// </summary>
        public static bool IsMessage(this Type messageType) =>
            messageType.GetInterfaces().Any(x => x.IsMessageInterface());

        /// <summary>
        ///     Verifies if provided <paramref name="handlerType" /> implements a message handler interface.
        /// </summary>
        public static bool IsMessageHandler(this Type handlerType) =>
            handlerType.GetInterfaces().Any(x => x.IsMessageHandlerInterface());

        /// <summary>
        ///     Verifies if provided <paramref name="messageType" /> is a message interface.
        /// </summary>
        private static bool IsMessageInterface(this Type messageType) =>
            messageType.IsInterface && messageType.IsGenericType && messageType.GetGenericTypeDefinition() == typeof(IMessage<>);

        /// <summary>
        ///     Verifies if provided <paramref name="handlerType" /> is a message handler interface.
        /// </summary>
        private static bool IsMessageHandlerInterface(this Type handlerType) =>
            handlerType.IsInterface && handlerType.IsGenericType && handlerType.GetGenericTypeDefinition() == typeof(IMessageHandler<,>);

        /// <summary>
        ///     Gets all implemented message interceptor interface types from message interceptor type.
        /// </summary>
        public static Type[] GetMessageInterceptorInterfaceTypes(this Type handlerType)
        {
            var abstractInterceptorTypes = handlerType.GetInterfaces().Where(x => x.IsMessageInterceptorInterface()).ToArray();
            if (abstractInterceptorTypes.Any())
                return abstractInterceptorTypes;

            if (handlerType.IsMessageInterceptorInterface())
                return new[] { handlerType };

            return Array.Empty<Type>();
        }

        /// <summary>
        ///     Verifies if provided <paramref name="interceptorType" /> implements a message handler interface.
        /// </summary>
        public static bool IsMessageInterceptor(this Type interceptorType) =>
            interceptorType.GetInterfaces().Any(x => x.IsMessageInterceptorInterface());
        
        /// <summary>
        ///     Verifies if provided <paramref name="interceptorType" /> is a message handler interface.
        /// </summary>
        private static bool IsMessageInterceptorInterface(this Type interceptorType) =>
            interceptorType.IsInterface && interceptorType.IsGenericType && interceptorType.GetGenericTypeDefinition() == typeof(IMessageInterceptor<,>);
    }
}
