using System.Collections.Generic;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Internal;
using System;

namespace Assistant.Net.Messaging
{
    public static class CommandOptionsExtensions
    {
        public static ISet<HandlerDefinition> AddRemote<TCommand>(this ISet<HandlerDefinition> set)
            where TCommand : class, IAbstractCommand => set.AddRemote(typeof(TCommand));

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