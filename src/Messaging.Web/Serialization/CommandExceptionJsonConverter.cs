using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Exceptions;
using System;
using System.Text.Json;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Json converter responsible for command exceptions serialization.
    /// </summary>
    public class CommandExceptionJsonConverter : ExceptionJsonConverter<CommandException>
    {
        /// <inheritdoc/>
        public CommandExceptionJsonConverter(ITypeEncoder typeEncoder) : base(typeEncoder) { }

        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert) => typeToConvert
            .IsAssignableTo(typeof(CommandException));

        /// <inheritdoc/>
        public override CommandException Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return base.Read(ref reader, typeToConvert, options);
            }
            catch(TypeResolvingFailedJsonException e)
            {
                return new UnknownCommandException(e.Type, e.Message, e.InnerException);
            }
        }
    }
}