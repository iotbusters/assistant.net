using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Options;

namespace Assistant.Net.Messaging;

/// <summary>
///     Messaging client configuration extensions for generic client.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures a storage based single message handler.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it has storage dependencies configured by <see cref="MessagingClientBuilder"/>.Use***()
    ///     or <see cref="StorageBuilder"/>.Use***() methods; the method overrides already configured single message handler.
    /// </remarks>
    public static MessagingClientBuilder UseGenericSingleHandler(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.UseGenericSingleHandler());
        return builder.AddConfiguration<GenericProxyConfiguration>();
    }

    /// <summary>
    ///     Configures a storage based backoff handling.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it has storage dependencies configured by <see cref="MessagingClientBuilder"/>.Use***()
    ///     or <see cref="StorageBuilder"/>.Use***() methods; the method overrides already configured backoff message handler.
    /// </remarks>
    public static MessagingClientBuilder UseGenericBackoffHandler(this MessagingClientBuilder builder)
    {
        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.UseGenericBackoffHandler());
        return builder.AddConfiguration<GenericProxyConfiguration>();
    }
}
