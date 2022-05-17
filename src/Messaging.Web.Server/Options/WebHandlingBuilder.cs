using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     WEB oriented message handling configuration builder on a server.
/// </summary>
public class WebHandlingBuilder : MessagingClientBuilder<WebHandlingBuilder>
{
    /// <summary/>
    public WebHandlingBuilder(IServiceCollection services) : base(services, WebOptionsNames.DefaultName) { }
}
