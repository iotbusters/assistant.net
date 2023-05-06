using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.DependencyInjection;
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
    public static GenericHandlingServerBuilder AddHandler<THandler>(this GenericHandlingServerBuilder builder) => builder
        .AddHandler(typeof(THandler));

    /// <summary>
    ///     Registers a <paramref name="handlerType"/> for external requests to the server.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="handlerType">The message handler implementation type.</param>
    /// <exception cref="ArgumentException"/>
    public static GenericHandlingServerBuilder AddHandler(this GenericHandlingServerBuilder builder, Type handlerType)
    {
        if (!handlerType.IsMessageHandler())
            throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

        builder.Services
            .ConfigureMessagingClient(builder.Name, b => b.AddHandler(handlerType))
            .ConfigureGenericHandlingServerOptions(builder.Name, o => o.AcceptMessagesFromHandler(handlerType));
        return builder;
    }

    /// <summary>
    ///     Registers a <paramref name="handlerInstance"/> for external requests to the server.
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
            .ConfigureMessagingClient(builder.Name, b => b.AddHandler(handlerInstance))
            .ConfigureGenericHandlingServerOptions(builder.Name, o => o.AcceptMessagesFromHandler(handlerType));
        return builder;
    }

    /// <summary>
    ///     Registers a <paramref name="handlerType"/> for external requests to the server if no other registered handlers.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered backoff handler.
    /// </remarks>
    /// <param name="builder"/>
    /// <param name="handlerType">The backoff message handler implementation type.</param>
    /// <exception cref="ArgumentException"/>
    public static GenericHandlingServerBuilder AddBackoffHandler(this GenericHandlingServerBuilder builder, Type handlerType)
    {
        builder.Services
            .ConfigureMessagingClient(builder.Name, b => b.UseBackoffHandler(handlerType))
            .ConfigureGenericHandlingServerOptions(builder.Name, o => o.BackoffHandler(true));
        return builder;
    }

    /// <summary>
    ///     Registers a <paramref name="handlerInstance"/> for external requests to the server if no other registered handlers.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered backoff handler.
    /// </remarks>
    /// <param name="builder"/>
    /// <param name="handlerInstance">The backoff message handler instance.</param>
    /// <exception cref="ArgumentException"/>
    public static GenericHandlingServerBuilder AddBackoffHandler(this GenericHandlingServerBuilder builder, IAbstractHandler handlerInstance)
    {
        builder.Services
            .ConfigureMessagingClient(builder.Name, b => b.UseBackoffHandler(handlerInstance))
            .ConfigureGenericHandlingServerOptions(builder.Name, o => o.BackoffHandler(true));
        return builder;
    }
}
