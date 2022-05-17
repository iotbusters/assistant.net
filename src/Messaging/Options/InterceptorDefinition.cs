using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Message interceptor definition.
/// </summary>
public class InterceptorDefinition
{
    private readonly Func<IServiceProvider, IAbstractInterceptor> factory;

    /// <summary/>
    public InterceptorDefinition(
        Type messageType,
        Type interceptorType,
        Func<IServiceProvider, IAbstractInterceptor> factory)
    {
        MessageType = messageType;
        InterceptorType = interceptorType;
        this.factory = factory;
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
    ///     Creates message interceptor instance.
    /// </summary>
    public IAbstractInterceptor Create(IServiceProvider provider) => factory(provider);
}