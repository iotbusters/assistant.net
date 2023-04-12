using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Storage based server message handling builder extensions.
/// </summary>
public static class GenericHandlingServerBuilderExtensions
{
    /// <summary>
    ///     Configures local provider for storage based messaging handling.
    /// </summary>
    public static GenericHandlingServerBuilder UseLocal(this GenericHandlingServerBuilder builder)
    {
        builder.Services.ConfigureStorage(GenericOptionsNames.DefaultName, b => b.UseLocal());
        return builder;
    }

    /// <summary>
    ///     Registers a server message handler type <typeparamref name="THandler"/>.
    /// </summary>
    /// <typeparam name="THandler">Message handler type.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static GenericHandlingServerBuilder AddHandler<THandler>(this GenericHandlingServerBuilder builder) => builder
        .AddHandler(typeof(THandler));

    /// <summary>
    ///     Registers a server message handler with <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerType">The message handler implementation type.</param>
    /// <exception cref="ArgumentException"/>
    public static GenericHandlingServerBuilder AddHandler(this GenericHandlingServerBuilder builder, Type handlerType)
    {
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        builder.Services
            .ConfigureMessagingClient(GenericOptionsNames.DefaultName, o => o.AddHandler(handlerType))
            .ConfigureGenericHandlingServerOptions(o => o.AcceptMessagesFromHandler(handlerType));
        return builder;
    }

    /// <summary>
    ///     Registers a server message handler with <paramref name="handlerInstance"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerInstance">The message handler instance.</param>
    /// <exception cref="ArgumentException"/>
    public static GenericHandlingServerBuilder AddHandler(this GenericHandlingServerBuilder builder, object handlerInstance)
    {
        var handlerType = handlerInstance.GetType();
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

        builder.Services
            .ConfigureMessagingClient(GenericOptionsNames.DefaultName, o => o.AddHandler(handlerInstance))
            .ConfigureGenericHandlingServerOptions(o => o.AcceptMessagesFromHandler(handlerType));
        return builder;
    }
}
