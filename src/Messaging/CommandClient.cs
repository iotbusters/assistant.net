using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging
{
    internal class CommandClient : ICommandClient
    {
        private readonly IHandlerFactory factory;

        public CommandClient(IHandlerFactory factory) =>
            this.factory = factory;

        public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command)
        {
            var handler = factory.Create(command.GetType());

            try
            {
                return (TResponse)await handler.Handle(command);
            }
            catch (CommandExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CommandFailedException(ex);
            }
        }
    }
}