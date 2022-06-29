using Assistant.Net.Options;
using System;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Message interceptor definition.
/// </summary>
public sealed class InterceptorDefinition<TInterceptor> where TInterceptor : class
{
    /// <summary/>
    public InterceptorDefinition(
        Type messageType,
        Type interceptorType,
        InstanceFactory<TInterceptor> factory)
    {
        MessageType = messageType;
        InterceptorType = interceptorType;
        Factory = factory;
    }

    /// <summary>
    ///     Accepting message type.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    ///     Message interceptor type.
    /// </summary>
    public Type InterceptorType { get; }

    /// <summary>
    ///     Interceptor instance factory.
    /// </summary>
    public InstanceFactory<TInterceptor> Factory { get; }
}
