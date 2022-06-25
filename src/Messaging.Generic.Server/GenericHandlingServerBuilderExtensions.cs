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
        builder.Services.ConfigureStorage(GenericOptionsNames.DefaultName, b => b.UseLocalSingleProvider());
        return builder;
    }

    /// <summary>
    ///     Registers a server message handler type <typeparamref name="THandler"/>.
    /// </summary>
    /// <typeparam name="THandler">Message handler type.</typeparam>
    public static GenericHandlingServerBuilder AddHandler<THandler>(this GenericHandlingServerBuilder builder) => builder
        .AddHandler(typeof(THandler));

    /// <summary>
    ///     Registers a server message handler with <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerType">The message handler implementation type.</param>
    public static GenericHandlingServerBuilder AddHandler(this GenericHandlingServerBuilder builder, Type handlerType)
    {
        builder.Services.ConfigureMessagingClient(GenericOptionsNames.DefaultName, o => o.AddHandler(handlerType));
        return builder;
    }

    /// <summary>
    ///     Registers a server message handler with <paramref name="handlerInstance"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerInstance">The message handler instance.</param>
    public static GenericHandlingServerBuilder AddHandler(this GenericHandlingServerBuilder builder, object handlerInstance)
    {
        builder.Services.ConfigureMessagingClient(GenericOptionsNames.DefaultName, o => o.AddHandler(handlerInstance));
        return builder;
    }
}
