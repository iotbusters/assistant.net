using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Json converter responsible for exception serialization.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0"/> />
    public class CommandExceptionJsonConverter : JsonConverter<CommandException>
    {
        private const string TypePropertyName = "type";
        private const string MessagePropertyName = "message";
        private const string InnerExceptionPropertyName = "inner";

        private readonly ILogger<CommandExceptionJsonConverter> logger;
        private readonly ITypeEncoder typeEncoder;

        public CommandExceptionJsonConverter(
            ILogger<CommandExceptionJsonConverter> logger,
            ITypeEncoder typeEncoder)
        {
            this.logger = logger;
            this.typeEncoder = typeEncoder;
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeof(Exception).IsAssignableFrom(typeToConvert);

        public override void Write(Utf8JsonWriter writer, CommandException value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value is UnknownCommandException ex)
                writer.WriteString(TypePropertyName, ex.Type);
            else
                writer.WriteString(TypePropertyName, typeEncoder.Encode(value.GetType()));

            writer.WriteString(MessagePropertyName, value.Message);

            if (value.InnerException as CommandException != null)
            {
                writer.WritePropertyName(InnerExceptionPropertyName);
                JsonSerializer.Serialize(writer, value.InnerException, options);
            }

            writer.WriteEndObject();
        }

        public override CommandException? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Start object token is expected.");

            var (encodedType, message, inner) = ReadExceptionContent(ref reader, options);

            if (encodedType == null)
                throw new JsonException($"Property '{TypePropertyName}' is required.");

            if (message == null)
                throw new JsonException($"Property '{MessagePropertyName}' is required.");

            try
            {
                var type = typeEncoder.Decode(encodedType!);
                var ctorArguments = new object?[] { message, inner }
                    .Where(x => x != null).Select(x => x!).ToArray();
                return (CommandException)Activator.CreateInstance(type!, ctorArguments)!;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cannot restore exception object by {Type}, {Message} and {InnerException}.", encodedType, message, inner);
                return new UnknownCommandException(encodedType, message, inner);
            }
        }

        private static (string? type, string? message, Exception? inner) ReadExceptionContent(
            ref Utf8JsonReader reader,
            JsonSerializerOptions options)
        {
            string? encodedType = null;
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
                        inner = JsonSerializer.Deserialize<Exception>(ref reader, options);
                        break;

                    default:
                        reader.Skip(); // ignore additional property values
                        break;
                }
            }

            return (encodedType, message, inner);
        }
    }
}