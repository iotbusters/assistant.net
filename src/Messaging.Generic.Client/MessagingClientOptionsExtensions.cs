using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Options;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client options extensions for storage based message handling client.
/// </summary>
public static class MessagingClientOptionsExtensions
{
    /// <summary>
    ///     Configures a storage based single message handler instance factory.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it has storage dependencies configured by <see cref="MessagingClientBuilder"/>.Use***()
    ///     or <see cref="StorageBuilder"/>.Use***() methods; the method overrides already configured single message handler.
    /// </remarks>
    public static MessagingClientOptions UseGenericSingleHandler(this MessagingClientOptions options) => options
        .UseSingleHandler((p, _) => p.Create<GenericMessagingHandlerProxy>());

    /// <summary>
    ///     Configures storage based backoff message handler instance factory.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it has storage dependencies configured by <see cref="MessagingClientBuilder"/>.Use***()
    ///     or <see cref="StorageBuilder"/>.Use***() methods; the method overrides already configured backoff message handler.
    /// </remarks>
    public static MessagingClientOptions UseGenericBackoffHandler(this MessagingClientOptions options) => options
        .UseBackoffHandler((p, _) => p.Create<GenericMessagingHandlerProxy>());
}
