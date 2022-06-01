using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using Assistant.Net.Serialization.Options;
using System;

namespace Assistant.Net.Serialization.Internal;

/// <summary>
///     Serializer factory responsible for resolving serializers due to a configuration.
/// </summary>
public class SerializerFactory : ISerializerFactory
{
    private readonly IServiceProvider provider;
    private readonly SerializerOptions options;

    /// <summary/>
    public SerializerFactory(IServiceProvider provider, INamedOptions<SerializerOptions> options)
    {
        this.provider = provider;
        this.options = options.Value;
    }

    /// <inheritdoc/>
    /// <exception cref="SerializerTypeNotRegisteredException"/>
    public IAbstractSerializer Create(Type serializingType)
    {
        if (options.Registrations.TryGetValue(serializingType, out var factory))
            return factory.Create(provider);

        if (options.AnyTypeRegistration != null)
            return options.AnyTypeRegistration.Create(provider, serializingType);

        throw new SerializerTypeNotRegisteredException(serializingType);
    }

    /// <inheritdoc/>
    /// <exception cref="SerializerTypeNotRegisteredException"/>
    public ISerializer<T> Create<T>() => (ISerializer<T>)Create(typeof(T));
}
