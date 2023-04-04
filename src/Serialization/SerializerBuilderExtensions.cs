using Assistant.Net.Options;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Options;
using System;

namespace Assistant.Net.Serialization;

/// <summary>
///     Serializer builder extensions for configuring JSON serialization.
/// </summary>
public static class SerializerBuilderExtensions
{
    /// <summary>
    ///     Configures single format <paramref name="serializerFactory"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered single format serializer.
    /// </remarks>
    public static SerializerBuilder UseFormat(this SerializerBuilder builder, Func<IServiceProvider, Type, IAbstractSerializer> serializerFactory)
    {
        builder.Services.ConfigureSerializerOptions(builder.Name, o =>
        {
            var factory = new InstanceCachingFactory<IAbstractSerializer, Type>(serializerFactory);
            o.SingleSerializer = factory;
        });
        return builder;
    }

    /// <summary>
    ///     Adds <typeparamref name="TValue"/> serialization.
    /// </summary>
    public static SerializerBuilder AddType<TValue>(this SerializerBuilder builder) => builder
        .AddType(typeof(TValue));

    /// <summary>
    ///     Adds <paramref name="serializingType" /> serialization.
    /// </summary>
    public static SerializerBuilder AddType(this SerializerBuilder builder, Type serializingType)
    {
        builder.Services.ConfigureSerializerOptions(builder.Name, o => o.Registrations.Add(serializingType));
        return builder;
    }

    /// <summary>
    ///     Allows serialization of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, <see cref="SerializerOptions.Registrations"/> will be ignored.
    /// </remarks>
    public static SerializerBuilder AllowAnyType(this SerializerBuilder builder)
    {
        builder.Services.ConfigureSerializerOptions(builder.Name, o => o.IsAnyTypeAllowed = true);
        return builder;
    }

    /// <summary>
    ///     Disallows serialization of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, <see cref="SerializerOptions.Registrations"/> should be configured. Default value: disallowed.
    /// </remarks>
    public static SerializerBuilder DisallowAnyType(this SerializerBuilder builder)
    {
        builder.Services.ConfigureSerializerOptions(builder.Name, o => o.IsAnyTypeAllowed = false);
        return builder;
    }

    /// <summary>
    ///     Removes <typeparamref name="TValue"/> type serialization.
    /// </summary>
    /// <typeparam name="TValue">Serializing value type.</typeparam>
    public static SerializerBuilder RemoveType<TValue>(this SerializerBuilder builder) => builder
        .RemoveType(typeof(TValue));

    /// <summary>
    ///     Removes <paramref name="serializingType"/> serialization.
    /// </summary>
    public static SerializerBuilder RemoveType(this SerializerBuilder builder, Type serializingType)
    {
        builder.Services.ConfigureSerializerOptions(builder.Name, o => o.Registrations.Remove(serializingType));
        return builder;
    }
}
