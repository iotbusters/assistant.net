using Assistant.Net.Messaging.Options;
using System;
using System.Net.Http;

namespace Assistant.Net.Messaging;

/// <summary>
///     WEB oriented messaging client configuration extensions.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures WEB based message handling.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already configured single message handler.
    /// </remarks>
    public static MessagingClientBuilder UseWeb(this MessagingClientBuilder builder)
    {
        builder.Services
            .ConfigureWebHandlerProxyOptions(builder.Name, delegate { })
            .ConfigureJsonSerialization(builder.Name)
            .AddWebMessageHandlerClient();
        return builder;
    }

    /// <summary>
    ///     Configures WEB based single message handling.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already configured single message handler.
    /// </remarks>
    /// <param name="builder"/>
    /// <param name="configure">The action used to configure the <see cref="HttpClient"/>.</param>
    public static MessagingClientBuilder UseWeb(this MessagingClientBuilder builder, Action<HttpClient> configure)
    {
        builder.Services.ConfigureWebHandlerProxyOptions(builder.Name, o => o.Configurations.Add(configure));
        return builder.UseWeb();
    }

    /// <summary>
    ///     Configures WEB based single message handling.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already configured single message handler.
    /// </remarks>
    public static MessagingClientBuilder UseWebSingleHandler(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.UseWebSingleHandler());
        return builder;
    }

    /// <summary>
    ///     Configures WEB based backoff message handling.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already configured backoff message handler.
    /// </remarks>
    public static MessagingClientBuilder UseWebBackoffHandler(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.UseWebBackoffHandler());
        return builder;
    }
}
