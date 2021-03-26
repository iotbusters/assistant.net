using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Messaging
{
    public sealed class CommandClient : ICommandClient
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IDictionary<Type, Type> handlers;

        public CommandClient(
            IServiceScopeFactory scopeFactory,
            IOptions<CommandOptions> options)
        {
            this.scopeFactory = scopeFactory;
            this.handlers = new Dictionary<Type, Type>(options.Value.Handlers);
        }

        public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command)
        {
            var commandType = command.GetType();
            if (!handlers.TryGetValue(commandType, out var handlerType))
                throw new InvalidOperationException($"No handler registered for {commandType}");

            using var scope = scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService(handlerType);
            var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand>.Handle));

            try
            {
                var task = (Task)handleMethod.Invoke(handler, new object[] { command });
                await task;
                return (TResponse)task.GetType().GetProperty(nameof(Task<object>.Result)).GetValue(task);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is CommandExecutionException cee)
            {
                ExceptionDispatchInfo.Capture(cee).Throw();
            }
            catch (TargetInvocationException ex)
            {
                throw new CommandFailedException(ex.InnerException);
            }
            catch (RuntimeBinderException ex)
            {
                throw new InvalidOperationException($"Cannot map command {command.GetType().Name} to handler {handler.GetType().Name}.", ex);
            }
            catch (Exception ex)
            {
                throw new CommandFailedException(ex);
            }
        }
    }
}