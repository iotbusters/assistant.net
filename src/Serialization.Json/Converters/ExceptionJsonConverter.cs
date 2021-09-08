using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Net.Serialization.Converters
{
    /// <summary>
    ///     Json converter responsible for exception serialization.
    /// </summary>
    /// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to
    public class ExceptionJsonConverter<T> : JsonConverter<T>
        where T : Exception
    {
        private const string TypePropertyName = "type";
        private const string MessagePropertyName = "message";
        private const string InnerExceptionPropertyName = "inner";

        private readonly ITypeEncoder typeEncoder;

        /// <summary/>
        public ExceptionJsonConverter(ITypeEncoder typeEncoder) =>
            this.typeEncoder = typeEncoder;

        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert) =>
            typeof(T).IsAssignableFrom(typeToConvert);

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var type = typeEncoder.Encode(value.GetType());
            writer.WriteString(TypePropertyName, type);

            writer.WriteString(MessagePropertyName, value.Message);

            if (value.InnerException != null && CanConvert(value.InnerException.GetType()))
            {
                writer.WritePropertyName(InnerExceptionPropertyName);
                JsonSerializer.Serialize(writer, value.InnerException, options);
            }

            writer.WriteEndObject();
        }

        /// <inheritdoc/>
        /// <exception cref="TypeResolvingFailedJsonException"/>
        /// <exception cref="JsonException"/>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Start object token is expected.");

            var (encodedType, message, inner) = ReadExceptionContent(ref reader, options);

            if (encodedType == null)
                throw new JsonException($"Property '{TypePropertyName}' is required.");

            if (message == null)
                throw new JsonException($"Property '{MessagePropertyName}' is required.");

            var type = typeEncoder.Decode(encodedType!);
            if (type == null)
                throw new TypeResolvingFailedJsonException(encodedType, message, inner);

            var ctorArguments = new object?[] { message, inner }
                .Where(x => x != null).Select(x => x!).ToArray();
            try
            {
                return (T)Activator.CreateInstance(type!, ctorArguments)!;
            }
            catch (Exception)
            {
                throw new TypeResolvingFailedJsonException(encodedType, message, inner);
            }
        }

        /// <exception cref="JsonException"/>
        private (string? type, string? message, T? inner) ReadExceptionContent(
            ref Utf8JsonReader reader,
            JsonSerializerOptions options)
        {
            string? encodedType = null;
            string? message = null;
            T? inner = null;

            for (reader.Read(); reader.TokenType != JsonTokenType.EndObject; reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Property name token is expected.");
                var propertyName = reader.GetString();

                reader.Read();
                switch (propertyName)
                {
                    case TypePropertyName:
                        if (reader.TokenType != JsonTokenType.String)
                            throw new JsonException("String token is expected.");
                        encodedType = reader.GetString();
                        break;

                    case MessagePropertyName:
                        if (reader.TokenType != JsonTokenType.String)
                            throw new JsonException("String token is expected.");
                        message = reader.GetString();
                        break;

                    case InnerExceptionPropertyName:
                        if (reader.TokenType != JsonTokenType.StartObject)
                            throw new JsonException("Start object token is expected.");

                        var exception = JsonSerializer.Deserialize<T>(ref reader, options);
                        if (exception != null && CanConvert(exception.GetType()))
                            inner = exception;
                        break;

                    // todo: implement
                    //case StackTracePropertyName: throw new NotSupportedException();

                    default:
                        reader.Skip(); // ignore additional property values
                        break;
                }
            }

            return (encodedType, message, inner);
        }
    }
}