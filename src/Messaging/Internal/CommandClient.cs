using Assistant.Net.Messaging.Abstractions;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Default command client implementation.
    /// </summary>
    internal class CommandClient : ICommandClient
    {
        private readonly IHandlerFactory factory;

        public CommandClient(IHandlerFactory factory) =>
            this.factory = factory;

        public Task<object> Send(object command) =>
            factory.Create(command.GetType()).Handle(command);
    }
}