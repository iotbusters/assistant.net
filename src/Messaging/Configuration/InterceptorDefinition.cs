using System;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Configuration
{
    public class InterceptorDefinition
    {
        internal InterceptorDefinition(Type type) => Type = type;

        public Type Type { get; }


        public static InterceptorDefinition Create<TInterceptor>() where TInterceptor : class, IAbstractCommandInterceptor =>
            new(typeof(TInterceptor));
    }
}