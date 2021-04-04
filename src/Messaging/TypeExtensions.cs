using System;
using System.Linq;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging
{
    public static class TypeExtensions
    {
        public static Type MakeGenericTypeBoundToCommand(this Type genericTypeDefinition, Type commandType)
        {
            if (!genericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException("Invalid generic type definition.", nameof(genericTypeDefinition));

            var responseType = commandType.GetResponseType()
                               ?? throw new ArgumentException("Invalid command type.", nameof(commandType));
            return genericTypeDefinition.MakeGenericType(commandType, responseType);
        }

        public static Type? GetResponseType(this Type commandType)
        {
            if (commandType.IsClass)
                return commandType.GetInterfaces().Select(x => x.GetResponseType()).SingleOrDefault(x => x != null);

            if (commandType.IsInterface && commandType.IsCommandInterface())
                return commandType.GetGenericArguments().Single();

            return null;
        }

        private static bool IsCommandInterface(this Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICommand<>);
    }
}
