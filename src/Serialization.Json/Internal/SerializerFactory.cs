using System;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;

namespace Assistant.Net.Serialization.Internal
{
    public class SerializerFactory : ISerializerFactory
    {
        private readonly IServiceProvider provider;

        public SerializerFactory(IServiceProvider provider) =>
            this.provider = provider;

        public ISerializer<object> Create(Type serializingType)
        {
            var serviceType = typeof(ISerializer<>).MakeGenericType(serializingType);
            dynamic serializer = provider.GetService(serviceType)
                                 ?? throw new SerializerTypeNotRegisteredException(serializingType);

            return new DelegatingAbstractSerializer(
                (s, x) => serializer.Serialize(s, (dynamic) x),
                async x => await serializer.Deserialize(x));
        }
    }
}