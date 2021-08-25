using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Exceptions;
using System;
using System.Text.Json;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Json converter responsible for message exceptions serialization.
    /// </summary>
    public class MessageExceptionJsonConverter : ExceptionJsonConverter<MessageException>
    {
        /// <inheritdoc/>
        public MessageExceptionJsonConverter(ITypeEncoder typeEncoder) : base(typeEncoder) { }

        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert) => typeToConvert
            .IsAssignableTo(typeof(MessageException));

        /// <inheritdoc/>
        public override MessageException Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
}