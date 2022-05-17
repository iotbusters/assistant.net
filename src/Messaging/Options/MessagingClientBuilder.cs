using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Regular messaging client builder implementation.
/// </summary>
public class MessagingClientBuilder : MessagingClientBuilder<MessagingClientBuilder>
{
    /// <summary/>
    public MessagingClientBuilder(IServiceCollection services, string name) : base(services, name) { }
}