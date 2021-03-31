using System.Collections.Generic;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;
using Assistant.Net.Messaging.Internal;

namespace Assistant.Net.Messaging.Web
{
    public static class CommandConfigurationBuilderExtensions
    {
        public static List<HandlerDefinition> AddRemote<TCommand>(this List<HandlerDefinition> list)
            where TCommand : class, IAbstractCommand
        {
            var type = typeof(CommandHandlerWebProxy<,>).MakeGenericTypeBoundToCommand(typeof(TCommand));
            list.Add(HandlerDefinition.Create(type));
            return list;
        }
    }
}