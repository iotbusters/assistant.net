using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     No handlers were registered for command type exception.
    /// </summary>
    public class CommandNotRegisteredException : CommandException
    {
        /// <summary/>
        public CommandNotRegisteredException() : base("Command wasn't registered.") { }

        /// <summary/>
        public CommandNotRegisteredException(string message) : base(message) { }

        /// <summary/>
        public CommandNotRegisteredException(Type commandType) : base($"Command '{commandType.Name}' wasn't registered.") { }
    }
}