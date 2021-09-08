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
        ///     and its type parameters resolved from <paramref name="messagingType" />
        /// </summary>
        /// <param name="genericTypeDefinition">Generic type definition that requires two parameters: message type and message response type.</param>
        /// <param name="messagingType">Specific message type.</param>
        /// <exception cref="ArgumentException"/>
        public static Type MakeGenericTypeBoundToMessage(this Type genericTypeDefinition, Type messagingType)
        {
            if (!genericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException("Invalid generic type definition.", nameof(genericTypeDefinition));

            var responseType = messagingType.GetResponseType()
                               ?? throw new ArgumentException("Invalid message type.", nameof(messagingType));
            return genericTypeDefinition.MakeGenericType(messagingType, responseType);
        }

        /// <summary>
        ///     Resolves message response type from message type.
        /// </summary>
        public static Type? GetResponseType(this Type messagingType)
        {
            if (messagingType.IsClass)
                return messagingType.GetInterfaces().Select(x => x.GetResponseType()).SingleOrDefault(x => x != null);

            if (messagingType.IsInterface && messagingType.IsMessageInterface())
                return messagingType.GetGenericArguments().Single();

            return null;
        }

        /// <summary>
        ///     Verifies if provided <paramref name="type" /> implements a message interface.
        /// </summary>
        private static bool IsMessageInterface(this Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IMessage<>);
    }
}
