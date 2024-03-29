using Assistant.Net.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assistant.Net.Serialization.Converters;

internal static class ExceptionJsonConverter
{
    public static bool CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(Exception));
}

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
    private const string StackTracePropertyName = "stacktrace";

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

        var stackTrace = new StackTrace(value, fNeedFileInfo: true).ToString();
        writer.WriteString(StackTracePropertyName, stackTrace);

        writer.WriteEndObject();
    }

    /// <inheritdoc/>
    /// <exception cref="NotResolvedJsonException"/>
    /// <exception cref="JsonException"/>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Start object token is expected.");

        var (encodedType, message, inner, stackTrace) = ReadExceptionContent(ref reader, options);

        if (encodedType == null)
            throw new JsonException($"Property '{TypePropertyName}' is required.");

        if (message == null)
            throw new JsonException($"Property '{MessagePropertyName}' is required.");

        if (stackTrace == null)
            throw new JsonException($"Property '{StackTracePropertyName}' is required.");

        var type = typeEncoder.Decode(encodedType);
        if (type == null)
            throw new ExceptionNotResolvedJsonException(
                $"Type '{encodedType}' wasn't found.",
                null,
                encodedType,
                message,
                inner);
        if (!CanConvert(type))
            throw new JsonException($"Unsupported by converter exception type `{type.Name}`.");

        var ctorArguments = new object?[] {message, inner}.Where(x => x != null).ToArray();
        try
        {
            var exception = (T)Activator.CreateInstance(type, ctorArguments)!;
            ExceptionDispatchInfo.SetRemoteStackTrace(exception, stackTrace);
            return exception;
        }
        catch(Exception ex)
        {
            throw new ExceptionNotResolvedJsonException(
                $"The type '{typeof(T)}' failed to deserialize.",
                ex,
                encodedType,
                message,
                inner);
        }
    }

    /// <exception cref="JsonException"/>
    private (string? type, string? message, T? inner, string? stackTrace) ReadExceptionContent(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options)
    {
        string? encodedType = null;
        string? message = null;
        T? inner = null;
        string? stackTrace = null;

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

                case StackTracePropertyName:
                    if (reader.TokenType != JsonTokenType.String)
                        throw new JsonException("String token is expected.");
                    stackTrace = reader.GetString();
                    break;

                default:
                    reader.Skip(); // ignore additional property values
                    break;
            }
        }

        return (encodedType, message, inner, stackTrace);
    }
}
