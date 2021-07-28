using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     A wrapper over interceptor type which is used during configuration.
    /// </summary>
    public class InterceptorDefinition
    {
        internal InterceptorDefinition(Type type) => Type = type;

        public Type Type { get; }


        public static InterceptorDefinition Create<TInterceptor>() where TInterceptor : class, IAbstractInterceptor =>
            new(typeof(TInterceptor));
    }
}