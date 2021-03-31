using System;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Configuration
{
    public class HandlerDefinition
    {

        internal HandlerDefinition(Type type) => Type = type;

        public Type Type { get; }

        public static HandlerDefinition Create<THandler>() where THandler : class, IAbstractCommandHandler =>
            new(typeof(THandler));

        public static HandlerDefinition Create(Type handlerType)
        {
            if (!handlerType.IsAssignableTo(typeof(IAbstractCommandHandler)))
                throw new ArgumentException("Invalid handler", nameof(handlerType));
            return new(handlerType);
        }
    }
}