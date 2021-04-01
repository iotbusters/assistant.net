using System;

namespace Assistant.Net.Messaging.Exceptions
{
    public class CommandNotRegisteredException : CommandException
    {
        public CommandNotRegisteredException(string message) : base(message) { }
        public CommandNotRegisteredException(Type commandType) : base($"Command {commandType} wasn't registered.") { }
    }
}