using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using Assistant.Net.Serialization.Options;
using System;

namespace Assistant.Net.Serialization.Internal;

/// <summary>
///     Serializer factory responsible for resolving serializers due to a configuration.
/// </summary>
public sealed class SerializerFactory : ISerializerFactory
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
    /// <exception cref="SerializerNotRegisteredException"/>
    /// <exception cref="SerializingTypeNotRegisteredException"/>
    public IAbstractSerializer Create(Type serializingType)
    {
        if (options.FormatSerializerFactory == null)
            throw new SerializerNotRegisteredException();

        if (!options.IsAnyTypeAllowed && !options.Registrations.Contains(serializingType))
            throw new SerializingTypeNotRegisteredException(serializingType);

        return options.FormatSerializerFactory.Create(provider, serializingType);
    }

    /// <inheritdoc/>
    /// <exception cref="SerializingTypeNotRegisteredException"/>
    public ISerializer<T> Create<T>() => (ISerializer<T>)Create(typeof(T));
}
