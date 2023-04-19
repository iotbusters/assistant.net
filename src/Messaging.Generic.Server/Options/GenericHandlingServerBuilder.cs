using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Storage based server message handling builder.
/// </summary>
public sealed class GenericHandlingServerBuilder
{
    /// <summary/>
    public GenericHandlingServerBuilder(IServiceCollection services, string name)
    {
        Services = services;
        Name = name;
    }

    /// <summary>
    ///     The name of the <see cref="GenericHandlingServerOptions"/> and <see cref="MessagingClientOptions"/> instances.
    /// </summary>
    public string Name { get; }

    /// <summary/>
    public IServiceCollection Services { get; }
}
