using System;
using System.Text.Json;

namespace Assistant.Net.Serialization.Exceptions
{
    public class TypeResolvingFailedJsonException : JsonException
    {
        public TypeResolvingFailedJsonException(string type, string message, Exception? innerException) : base(message, innerException) =>
            Type = type;

        public string Type { get; }
    }
}