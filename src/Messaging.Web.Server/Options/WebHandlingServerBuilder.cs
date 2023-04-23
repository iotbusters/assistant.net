using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     WEB based server message handling builder.
/// </summary>
public class WebHandlingServerBuilder
{
    /// <summary/>
    public WebHandlingServerBuilder(IServiceCollection services, string name)
    {
        Services = services;
        Name = name;
    }

    /// <summary>
    ///     The name of the <see cref="WebHandlingServerOptions"/> and <see cref="MessagingClientOptions"/> instances.
    /// </summary>
    public string Name { get; }

    /// <summary/>
    public IServiceCollection Services { get; }
}
