using System;

namespace Assistant.Net.Serialization.Exceptions
{
    public class SerializerTypeNotRegisteredException : Exception
    {
        public SerializerTypeNotRegisteredException(string message) : base(message){ }
        public SerializerTypeNotRegisteredException(Type serializingType) : base($"Type '{serializingType.Name}' wasn't registered.") { }
    }
}