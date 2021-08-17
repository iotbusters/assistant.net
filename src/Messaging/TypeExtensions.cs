using Assistant.Net.Messaging.Abstractions;
using System;
using System.Linq;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     System.Type extensions for command handling.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Makes a generic type from the definition <paramref name="genericTypeDefinition" />
        ///     and its type parameters resolved from <paramref name="commandType" />
        /// </summary>
        /// <param name="genericTypeDefinition">Generic type definition that requires two parameters: command type and command response type.</param>
        /// <param name="commandType">Specific command type.</param>
        public static Type MakeGenericTypeBoundToCommand(this Type genericTypeDefinition, Type commandType)
        {
            if (!genericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException("Invalid generic type definition.", nameof(genericTypeDefinition));

            var responseType = commandType.GetResponseType()
                               ?? throw new ArgumentException("Invalid command type.", nameof(commandType));
            return genericTypeDefinition.MakeGenericType(commandType, responseType);
        }

        /// <summary>
        ///     Resolves command response type from command type.
        /// </summary>
        public static Type? GetResponseType(this Type commandType)
        {
            if (commandType.IsClass)
                return commandType.GetInterfaces().Select(x => x.GetResponseType()).SingleOrDefault(x => x != null);

            if (commandType.IsInterface && commandType.IsCommandInterface())
                return commandType.GetGenericArguments().Single();

            return null;
        }

        /// <summary>
        ///     Verifies if provided <paramref name="type" /> implements a command interface.
        /// </summary>
        private static bool IsCommandInterface(this Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICommand<>);
    }
}
