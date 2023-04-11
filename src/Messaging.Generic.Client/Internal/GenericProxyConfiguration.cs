using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Storage single provider configured server proxy message handling.
/// </summary>
public sealed class GenericProxyConfiguration : IMessageConfiguration
{
    /// <inheritdoc/>
    public void Configure(MessagingClientBuilder builder) => builder.Services
        .AddStorage(builder.Name, b => b
            .Add<string, CachingResult>() // GenericMessagingHandlerProxy's requirement
            .Add<string, IAbstractMessage>() // GenericMessagingHandlerProxy's requirement
            .Add<string, RemoteHandlerModel>()); // GenericMessagingHandlerProxy's requirement
}
