using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Internal;
using ProtoBuf;
using System;

namespace Assistant.Net.Serialization;

/// <summary>
///     Serializer builder extensions for configuring ProtoBuf serialization.
/// </summary>
public static class SerializerBuilderExtensions
{
    /// <summary>
    ///     Configures single ProtoBuf serializer.
    /// </summary>
    /// <remarks>
    ///     All registered serializing types will be optimized lazily;
    ///     in order to optimize that you can use dedicated registration method <see cref="AddTypeProto"/>.
    /// </remarks>
    public static SerializerBuilder UseProto(this SerializerBuilder builder) => builder
        .UseFormat((provider, serializingType) =>
        {
            var implementationType = typeof(DefaultProtoSerializer<>).MakeGenericType(serializingType);
            return (IAbstractSerializer)provider.Create(implementationType);
        });

    /// <summary>
    ///     Adds <paramref name="serializingType" /> serialization and ProtoBuf pre-compilation.
    /// </summary>
    public static SerializerBuilder AddTypeProto(this SerializerBuilder builder, Type serializingType)
    {
        // todo: replace with type argument once it's added.
        var prepareSerializerTemplate = typeof(Serializer).GetMethod(nameof(Serializer.PrepareSerializer));
        var prepareSerializer = prepareSerializerTemplate!.MakeGenericMethod(serializingType);
        prepareSerializer.Invoke(obj: null, parameters: null);

        return builder.AddType(serializingType);
    }

    /// <summary>
    ///     Adds <typeparamref name="TValue"/> serialization and ProtoBuf pre-compilation.
    /// </summary>
    public static SerializerBuilder AddTypeProto<TValue>(this SerializerBuilder builder)
    {
        Serializer.PrepareSerializer<TValue>();
        return builder.AddType<TValue>();
    }
}
