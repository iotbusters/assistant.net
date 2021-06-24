using System;
using System.Text.Json;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Exceptions;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Json converter responsible for command exceptions serialization.
    /// </summary>
    public class CommandExceptionJsonConverter : ExceptionJsonConverter<CommandException>
    {
        public CommandExceptionJsonConverter(ITypeEncoder typeEncoder) : base(typeEncoder) { }

        public override bool CanConvert(Type typeToConvert) => typeToConvert
            .IsAssignableTo(typeof(CommandException));

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