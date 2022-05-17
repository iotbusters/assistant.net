using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Exceptions;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;

namespace Assistant.Net.Messaging.Serialization;

/// <summary>
///     Json converter responsible for message exceptions serialization.
/// </summary>
public class MessageExceptionJsonConverter : ExceptionJsonConverter<Exception>
{
    private readonly IOptions<MessagingClientOptions> clientOptions;

    /// <inheritdoc/>
    public MessageExceptionJsonConverter(
        ITypeEncoder typeEncoder,
        IOptions<MessagingClientOptions> clientOptions)
        : base(typeEncoder) =>
        this.clientOptions = clientOptions;

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) =>
        base.CanConvert(typeToConvert)
        && typeToConvert.IsAssignableTo(typeof(MessageException))
        || clientOptions.Value.ExposedExceptions.Any(x => x.IsAssignableFrom(typeToConvert));

    /// <inheritdoc/>
    public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return base.Read(ref reader, typeToConvert, options);
        }
        catch(TypeResolvingFailedJsonException e)
        {
            return new UnknownMessageException(e.Type, e.Message, e.InnerException);
        }
    }
}
