using System.Collections.Generic;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;
using Assistant.Net.Messaging.Internal;

namespace Assistant.Net.Messaging
{
    public static class CommandOptionsExtensions
    {
        public static ISet<HandlerDefinition> AddRemote<TCommand>(this ISet<HandlerDefinition> set)
            where TCommand : class, IAbstractCommand
        {
            var type = typeof(CommandHandlerWebProxy<,>).MakeGenericTypeBoundToCommand(typeof(TCommand));
            set.Add(HandlerDefinition.Create(type));
            return set;
        }
    }
}