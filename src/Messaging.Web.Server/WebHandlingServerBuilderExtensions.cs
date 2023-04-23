using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Storage based server message handling builder extensions.
/// </summary>
public static class WebHandlingServerBuilderExtensions
{
    /// <summary>
    ///     Configures local provider for storage used in some interceptors.
    /// </summary>
    public static WebHandlingServerBuilder UseLocal(this WebHandlingServerBuilder builder)
    {
        builder.Services
            .ConfigureStorage(builder.Name, b => b.UseLocal())
            .AddHealthChecks()
            .AddLocalStorage(builder.Name);
        return builder;
    }

    /// <summary>
    ///     Registers a server message handler type <typeparamref name="THandler"/>.
    /// </summary>
    /// <typeparam name="THandler">Message handler type.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static WebHandlingServerBuilder AddHandler<THandler>(this WebHandlingServerBuilder builder) => builder
        .AddHandler(typeof(THandler));

    /// <summary>
    ///     Registers a server message handler with <paramref name="handlerType"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerType">The message handler implementation type.</param>
    /// <exception cref="ArgumentException"/>
    public static WebHandlingServerBuilder AddHandler(this WebHandlingServerBuilder builder, Type handlerType)
    {
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        builder.Services
            .ConfigureMessagingClient(builder.Name, o => o.AddHandler(handlerType))
            .ConfigureWebHandlingServerOptions(builder.Name, o => o.AcceptMessagesFromHandler(handlerType));
        return builder;
    }

    /// <summary>
    ///     Registers a server message handler with <paramref name="handlerInstance"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerInstance">The message handler instance.</param>
    /// <exception cref="ArgumentException"/>
    public static WebHandlingServerBuilder AddHandler(this WebHandlingServerBuilder builder, object handlerInstance)
    {
        var handlerType = handlerInstance.GetType();
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

        builder.Services
            .ConfigureMessagingClient(builder.Name, o => o.AddHandler(handlerInstance))
            .ConfigureWebHandlingServerOptions(builder.Name, o => o.AcceptMessagesFromHandler(handlerType));
        return builder;
    }
}
