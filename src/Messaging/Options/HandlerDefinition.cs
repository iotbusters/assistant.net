using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     A wrapper over handler type which is used during configuration.
    /// </summary>
    public class HandlerDefinition
    {
        internal HandlerDefinition(Type type) => Type = type;

        public Type Type { get; }

        public static HandlerDefinition Create<THandler>() where THandler : class, IAbstractHandler =>
            new(typeof(THandler));

        public static HandlerDefinition Create(Type handlerType)
        {
            if (!handlerType.IsAssignableTo(typeof(IAbstractHandler)))
                throw new ArgumentException("Invalid handler", nameof(handlerType));
            return new(handlerType);
        }
    }
}