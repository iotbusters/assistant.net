using System;
using System.Text.Json;

namespace Assistant.Net.Serialization.Exceptions
{
    /// <summary>
    ///     An exception thrown if some type cannot be resolved during deserialization.
    /// </summary>
    public class TypeResolvingFailedJsonException : JsonException
    {
        /// <summary/>
        public TypeResolvingFailedJsonException(string type, string message, Exception? innerException) : base(message, innerException) =>
            Type = type;

        /// <summary>
        ///     Unknown type which wasn't resolved.
        /// </summary>
        public string Type { get; }
    }
}