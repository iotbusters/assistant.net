using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;

namespace Assistant.Net.Serialization.Internal
{
    public class DefaultSerializerFactory : ISerializerFactory
    {
        private readonly IServiceProvider provider;

        public DefaultSerializerFactory(IServiceProvider provider) =>
            this.provider = provider;

        public ISerializer<object> Create(Type serializingType)
        {
            var serviceType = typeof(ISerializer<>).MakeGenericType(serializingType);
            dynamic serializer =  provider.GetService(serviceType)
                ?? throw new SerializerTypeNotRegisteredException(serializingType);

            return new DelegatingAbstractSerializer(o => serializer.Serialize((dynamic)o), b => serializer.Deserialize(b));
        }

        public ISerializer<TValue> Create<TValue>() =>
            provider.GetService<ISerializer<TValue>>()
            ?? throw new SerializerTypeNotRegisteredException(typeof(TValue));
    }
}