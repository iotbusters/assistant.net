using Assistant.Net.Messaging.Abstractions;
using System;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Message handler definition.
/// </summary>
public class HandlerDefinition
{
    private readonly Func<IServiceProvider, IAbstractHandler> factory;

    /// <summary/>
    public HandlerDefinition(Func<IServiceProvider, IAbstractHandler> factory) =>
        this.factory = factory;

    /// <summary>
    ///     Creates message handler instance.
    /// </summary>
    public IAbstractHandler Create(IServiceProvider provider) => factory(provider);
}