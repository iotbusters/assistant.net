using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Exceptions;
using System;
using System.Linq;
using System.Text.Json;

namespace Assistant.Net.Messaging.Serialization;

/// <summary>
///     Json converter responsible for message exceptions serialization.
/// </summary>
public class MessageExceptionJsonConverter : ExceptionJsonConverter<Exception>
{
    private readonly MessagingClientOptions clientOptions;

    /// <inheritdoc/>
    public MessageExceptionJsonConverter(
        ITypeEncoder typeEncoder,
        INamedOptions<MessagingClientOptions> clientOptions) : base(typeEncoder) =>
        this.clientOptions = clientOptions.Value;

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) =>
        base.CanConvert(typeToConvert)
        && typeToConvert.IsAssignableTo(typeof(MessageException))
        || clientOptions.ExposedExceptions.Any(x => x.IsAssignableFrom(typeToConvert));

    /// <inheritdoc/>
    public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return base.Read(ref reader, typeToConvert, options);
        }
        catch(ExceptionNotResolvedJsonException ex)
        {
            return new UnknownMessageException(ex.TargetType, ex.TargetMessage, ex.TargetInner);
        }
    }
}
