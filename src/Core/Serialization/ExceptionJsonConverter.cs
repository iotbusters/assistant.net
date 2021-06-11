using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Assistant.Net.Abstractions;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Json converter responsible for exception serialization.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0"/> />
    public class ExceptionJsonConverter<TValue> : JsonConverter<TValue>
        where TValue : Exception
    {
        private const string TypePropertyName = "type";
        private const string MessagePropertyName = "message";
        private const string InnerExceptionPropertyName = "inner";

        private readonly ITypeEncoder typeEncoder;

        public ExceptionJsonConverter(ITypeEncoder typeEncoder) =>
            this.typeEncoder = typeEncoder;

        public override bool CanConvert(Type typeToConvert) =>
            typeof(TValue).IsAssignableFrom(typeToConvert);

        public override void Write(Utf8JsonWriter writer, TValue value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString(TypePropertyName, GetType(value));

            writer.WriteString(MessagePropertyName, GetMessage(value));

            if (value.InnerException is TValue inner)
            {
                writer.WritePropertyName(InnerExceptionPropertyName);
                JsonSerializer.Serialize(writer, inner, options);
            }

            writer.WriteEndObject();
        }

        public override TValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                return (TValue)Activator.CreateInstance(type!, ctorArguments)!;
            }
            catch (Exception)
            {
                return DefaultException(encodedType, message, inner);
            }
        }

        protected virtual string GetType(TValue value) => typeEncoder.Encode(value.GetType());

        protected virtual string GetMessage(TValue value) => value.Message;

        protected virtual TValue? DefaultException(string type, string message, Exception? inner) => null;

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
                        inner = JsonSerializer.Deserialize<TValue>(ref reader, options);
                        break;

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