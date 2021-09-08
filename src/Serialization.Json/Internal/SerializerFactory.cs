using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using System;

namespace Assistant.Net.Serialization.Internal
{
    /// <summary>
    ///     Serializer factory responsible for resolving serializers due to a configuration.
    /// </summary>
    public class SerializerFactory : ISerializerFactory
    {
        private readonly IServiceProvider provider;

        /// <summary/>
        public SerializerFactory(IServiceProvider provider) =>
            this.provider = provider;

        /// <summary>
        ///     Resolves an instance of serializer for <paramref name="serializingType"/>.
        /// </summary>
        /// <exception cref="SerializerTypeNotRegisteredException"/>
        public IAbstractSerializer Create(Type serializingType)
        {
            var serviceType = typeof(ISerializer<>).MakeGenericType(serializingType);
            return (IAbstractSerializer?) provider.GetService(serviceType)
                   ?? throw new SerializerTypeNotRegisteredException(serializingType);
        }
    }
}