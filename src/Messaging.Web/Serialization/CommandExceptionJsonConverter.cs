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
    /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0"/> />
    public class CommandExceptionJsonConverter : ExceptionJsonConverter
    {
        public CommandExceptionJsonConverter(ITypeEncoder typeEncoder) : base(typeEncoder) { }

        public override bool CanConvert(Type typeToConvert) => typeToConvert
            .IsAssignableTo(typeof(CommandException));

        public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return base.Read(ref reader, typeToConvert, options);
            }
            catch(InstantiateFailedJsonException e)
            {
                return new UnknownCommandException(e.Type, e.Message, e.InnerException);
            }
        }
    }
}