using System;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Configuration
{
    public class HandlerDefinition
    {

        internal HandlerDefinition(Type type) => Type = type;

        internal Type Type { get; }

        public static HandlerDefinition Create<THandler>() where THandler : class, ICommandHandler =>
            new(typeof(THandler));

    }
}