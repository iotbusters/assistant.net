using System;

namespace Assistant.Net.Messaging.Exceptions
{
    /// <summary>
    ///     No handlers were registered for command type exception.
    /// </summary>
    public class CommandNotRegisteredException : CommandException
    {
        public CommandNotRegisteredException() : base($"Command wasn't registered.") { }
        public CommandNotRegisteredException(string message) : base(message) { }
        public CommandNotRegisteredException(Type commandType) : base($"Command '{commandType.Name}' wasn't registered.") { }
    }
}