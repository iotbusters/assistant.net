using Assistant.Net.Serialization.Abstractions;
using System;
using System.Linq;

namespace Assistant.Net.Serialization;

/// <summary>
///     System.Type extensions for messaging handling.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Resolves serializing type from serializer type.
    /// </summary>
    public static Type? GetSerializingType(this Type serializerType) =>
        serializerType.IsSerializerInterface()
            ? serializerType.GetGenericArguments().Single()
            : null;

    /// <summary>
    ///     Verifies if provided <paramref name="serializerType"/> is the <see cref="ISerializer{TValue}"/>.
    /// </summary>
    public static bool IsSerializerInterface(this Type serializerType) =>
        serializerType is {IsInterface: true, IsGenericType: true} && serializerType.GetGenericTypeDefinition() == typeof(ISerializer<>);

    /// <summary>
    ///     Verifies if provided <paramref name="serializerType"/> implements <see cref="ISerializer{TValue}"/>.
    /// </summary>
    public static bool IsAbstractSerializer(this Type serializerType) =>
        serializerType.IsAssignableTo(typeof(IAbstractSerializer));

    /// <summary>
    ///     Gets all serializing types of the <see cref="ISerializer{TValue}"/> types implemented by <paramref name="serializerType"/>.
    /// </summary>
    public static Type[] GetSerializingTypes(this Type serializerType) =>
        serializerType.GetInterfaces().Select(GetSerializingType).Where(x => x != null).Distinct().ToArray()!;
}
