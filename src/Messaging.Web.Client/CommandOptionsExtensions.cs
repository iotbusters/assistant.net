using System;
using System.Collections.Generic;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Internal;

namespace Assistant.Net.Messaging
{
    public static class CommandOptionsExtensions
    {
        /// <summary>
        ///     Registers remote handling for <typeparamref name="TCommand" />.
        /// </summary>
        /// <param name="set">Set of existing command handler definitions.</param>
        /// <typeparam name="TCommand">Specific command type to be handled remotely.</typeparam>
        public static ISet<HandlerDefinition> AddRemote<TCommand>(this ISet<HandlerDefinition> set)
            where TCommand : class, IAbstractCommand => set.AddRemote(typeof(TCommand));

        /// <summary>
        ///     Registers remote handling for <paramref name="commandType" />.
        /// </summary>
        /// <param name="set">Set of existing command handler definitions.</param>
        /// <param name="commandType">Specific command type to be handled remotely.</param>
        public static ISet<HandlerDefinition> AddRemote(this ISet<HandlerDefinition> set, Type commandType)
        {
            if (commandType.GetResponseType() == null)
                throw new ArgumentException("Invalid command type.", nameof(commandType));

            var type = typeof(CommandHandlerWebProxy<,>).MakeGenericTypeBoundToCommand(commandType);
            set.Add(HandlerDefinition.Create(type));
            return set;
        }
    }
}