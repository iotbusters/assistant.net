using System;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Json converter responsible for command exceptions serialization.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0"/> />
    public class CommandExceptionJsonConverter : ExceptionJsonConverter<CommandException>
    {
        public CommandExceptionJsonConverter(ITypeEncoder typeEncoder) : base(typeEncoder) { }

        protected override string GetType(CommandException value)
        {
            if(value is UnknownCommandException ex)
                return ex.Type;
            return base.GetType(value);
        }

        protected override CommandException? DefaultException(string type, string message, Exception? inner) =>
            new UnknownCommandException(type, message, inner);
    }
}