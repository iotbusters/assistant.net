using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions for WEB client.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Configures WEB based single message handler instance factory.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already configured single message handler.
    /// </remarks>
    public static MessagingClientOptions UseWebSingleHandler(this MessagingClientOptions options) => options
        .UseSingleHandler((p, _) => p.Create<WebMessageHandlerProxy>());

    /// <summary>
    ///     Configures WEB based backoff message handler instance factory.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already configured backoff message handler.
    /// </remarks>
    public static MessagingClientOptions UseWebBackoffHandler(this MessagingClientOptions options) => options
        .UseBackoffHandler((p, _) => p.Create<WebMessageHandlerProxy>());
}
