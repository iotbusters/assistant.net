using Assistant.Net.Options;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using System;
using System.Linq;

namespace Assistant.Net.Serialization;

/// <summary>
///     Serializer builder extensions for configuring JSON serialization.
/// </summary>
public static class SerializerBuilderExtensions
{
    /// <summary>
    ///     Adds all serializing types implemented by <typeparamref name="TSerializer"/> type.
    /// </summary>
    /// <typeparam name="TSerializer">Serializer type.</typeparam>
    public static SerializerBuilder AddSerializer<TSerializer>(this SerializerBuilder builder) where TSerializer : IAbstractSerializer => builder
        .AddSerializer(typeof(TSerializer));

    /// <summary>
    ///     Adds all serializing types implemented by <paramref name="serializer"/> object.
    /// </summary>
    public static SerializerBuilder AddSerializer(this SerializerBuilder builder, IAbstractSerializer serializer)
    {
        var serializerType = serializer.GetType();
        var serializingTypes = serializerType.GetSerializingTypes();
        if (!serializingTypes.Any())
            throw new ArgumentException($"{serializerType} doesn't implement any {typeof(ISerializer<>)}.");

        builder.Services.ConfigureSerializerOptions(builder.Name, o =>
        {
            foreach (var serializingType in serializingTypes)
                o.Registrations.Add(serializingType, new InstanceCachingFactory<IAbstractSerializer>(_ => serializer));
        });
        return builder;
    }

    /// <summary>
    ///     Adds all serializing types implemented by <paramref name="serializerType"/>.
    /// </summary>
    public static SerializerBuilder AddSerializer(this SerializerBuilder builder, Type serializerType)
    {
        var serializingTypes = serializerType.GetSerializingTypes();
        if (!serializingTypes.Any())
            throw new ArgumentException($"{serializerType} doesn't implement any {typeof(ISerializer<>)}.");

        builder.Services.ConfigureSerializerOptions(builder.Name, o =>
        {
            var factory = new InstanceCachingFactory<IAbstractSerializer>(p => (IAbstractSerializer)p.Create(serializerType));
            foreach (var serializingType in serializingTypes)
                o.Registrations.Add(serializingType, factory);
        });
        return builder;
    }

    /// <summary>
    ///     Adds <paramref name="serializingType"/> serialization.
    /// </summary>
    /// <remarks>
    ///     Pay attention, a <paramref name="serializer"/> object should support <paramref name="serializingType"/>.
    /// </remarks>
    public static SerializerBuilder AddType(this SerializerBuilder builder, Type serializingType, IAbstractSerializer serializer) => builder
        .AddType(serializingType, _ => serializer);

    /// <summary>
    ///     Adds <paramref name="serializingType" /> serialization.
    /// </summary>
    public static SerializerBuilder AddType(this SerializerBuilder builder, Type serializingType, Type serializerType)
    {
        if (!serializerType.IsAbstractSerializer())
            throw new ArgumentException($"{serializerType} doesn't implement {typeof(IAbstractSerializer)}.");

        return builder.AddType(serializingType, p => (IAbstractSerializer)p.Create(serializerType));
    }

    /// <summary>
    ///     Adds <paramref name="serializingType" /> serialization.
    /// </summary>
    /// <remarks>
    ///     Pay attention, a serializer created by <paramref name="serializerFactory"/> should support <paramref name="serializingType"/>.
    /// </remarks>
    public static SerializerBuilder AddType(this SerializerBuilder builder, Type serializingType, Func<IServiceProvider, object> serializerFactory)
    {
        builder.Services.ConfigureSerializerOptions(builder.Name, o =>
        {
            var factory = new InstanceCachingFactory<IAbstractSerializer>(p => (IAbstractSerializer)serializerFactory(p));
            o.Registrations.Add(serializingType, factory);
        });
        return builder;
    }

    /// <summary>
    ///     Adds serialization of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, <paramref name="serializer"/> object should support any type.
    /// </remarks>
    public static SerializerBuilder AddTypeAny(this SerializerBuilder builder, IAbstractSerializer serializer) => builder
        .AddTypeAny((_, _) => serializer);

    /// <summary>
    ///     Adds serialization of any type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, a serializer created by <paramref name="serializerFactory"/> should support any type.
    /// </remarks>
    public static SerializerBuilder AddTypeAny(this SerializerBuilder builder, Func<IServiceProvider, Type, IAbstractSerializer> serializerFactory)
    {
        builder.Services.ConfigureSerializerOptions(builder.Name, o =>
        {
            var factory = new InstanceCachingFactory<IAbstractSerializer, Type>(serializerFactory);
            o.AnyTypeRegistration = factory;
        });
        return builder;
    }

    /// <summary>
    ///     Removes <typeparamref name="TValue"/> type serialization.
    /// </summary>
    /// <typeparam name="TValue">Serializing value type.</typeparam>
    public static SerializerBuilder Remove<TValue>(this SerializerBuilder builder) => builder
        .Remove(typeof(TValue));

    /// <summary>
    ///     Removes <paramref name="serializingType"/> serialization.
    /// </summary>
    public static SerializerBuilder Remove(this SerializerBuilder builder, Type serializingType)
    {
        builder.Services.ConfigureSerializerOptions(builder.Name, o => o.Registrations.Remove(serializingType));
        return builder;
    }
}
