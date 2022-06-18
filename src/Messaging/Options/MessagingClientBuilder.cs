using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Messaging client builder implementation.
/// </summary>
public sealed class MessagingClientBuilder
{
    /// <summary/>
    public MessagingClientBuilder(IServiceCollection services, string name)
    {
        Services = services;
        Name = name;
    }

    /// <summary>
    ///     The name of the <see cref="MessagingClientOptions"/> instance.
    /// </summary>
    public string Name { get; }

    /// <summary/>
    public IServiceCollection Services { get; }
}
