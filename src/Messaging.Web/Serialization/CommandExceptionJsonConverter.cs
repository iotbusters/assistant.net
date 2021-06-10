using System;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Serialization
{
    /// <summary>
    ///     Json converter responsible for exception serialization.
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

        protected override Exception? GetInnerException(CommandException value)
        {
            if (value.InnerException as CommandException == null)
                return null;
            return base.GetInnerException(value);
        }
    }
}