using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.Extensions.Logging;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Json converter responsible for exception serialization.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0"/> />
    public class ExceptionJsonConverter : JsonConverter<Exception>
    {
        private const string TypePropertyName = "type";
        private const string MessagePropertyName = "message";
        private const string InnerExceptionPropertyName = "inner";

        private readonly ILogger<ExceptionJsonConverter> logger;

        public ExceptionJsonConverter(ILogger<ExceptionJsonConverter> logger) =>
            this.logger = logger;

        public override bool CanConvert(Type typeToConvert) =>
            typeof(Exception).IsAssignableFrom(typeToConvert);

        public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value is UnknownCommandException ex)
                writer.WriteString(TypePropertyName, ex.Type);
            else
                writer.WriteString(TypePropertyName, value.GetType().AssemblyQualifiedName);

            writer.WriteString(MessagePropertyName, value.Message);

            if (value.InnerException != null)
            {
                writer.WritePropertyName(InnerExceptionPropertyName);
                JsonSerializer.Serialize(writer, value.InnerException, options);
            }

            writer.WriteEndObject();
        }

        public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Start object token is expected.");

            var (typeName, message, inner) = ReadExceptionContent(ref reader, options);

            if (typeName == null)
                throw new JsonException($"Property '{TypePropertyName}' cannot be null.");

            if (message == null)
                throw new JsonException($"Property '{MessagePropertyName}' cannot be null.");

            try
            {
                var type = Type.GetType(typeName!, throwOnError: true);
                var ctorArguments = new object?[] { message, inner }
                    .Where(x => x != null).Select(x => x!).ToArray();
                return (Exception)Activator.CreateInstance(type!, ctorArguments)!;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cannot reproduce exception by {Type}, {Message} and {InnerException}.", typeName, message, inner);
                return new UnknownCommandException(typeName, message, inner);
            }
        }

        private static (string? type, string? message, Exception? inner) ReadExceptionContent(
            ref Utf8JsonReader reader,
            JsonSerializerOptions options)
        {
            string? typeName = null;
            string? message = null;
            Exception? inner = null;

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
                        typeName = reader.GetString();
                        break;

                    case MessagePropertyName:
                        if (reader.TokenType != JsonTokenType.String)
                            throw new JsonException("String token is expected.");
                        message = reader.GetString();
                        break;

                    case InnerExceptionPropertyName:
                        if (reader.TokenType != JsonTokenType.StartObject)
                            throw new JsonException("Start object token is expected.");
                        inner = JsonSerializer.Deserialize<Exception>(ref reader, options);
                        break;

                    default:
                        reader.Skip(); // ignore additional property values
                        break;
                }
            }

            return (typeName, message, inner);
        }
    }
}