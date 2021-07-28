using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using System;

namespace Assistant.Net.Serialization.Internal
{
    public class SerializerFactory : ISerializerFactory
    {
        private readonly IServiceProvider provider;

        public SerializerFactory(IServiceProvider provider) =>
            this.provider = provider;

        public IAbstractSerializer Create(Type serializingType)
        {
            var serviceType = typeof(ISerializer<>).MakeGenericType(serializingType);
            return (IAbstractSerializer?) provider.GetService(serviceType)
                   ?? throw new SerializerTypeNotRegisteredException(serializingType);
        }
    }
}