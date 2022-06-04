using Assistant.Net.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Messaging client builder implementation.
/// </summary>
public class MessagingClientBuilder : IBuilder<MessagingClientBuilder>
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

    MessagingClientBuilder IBuilder<MessagingClientBuilder>.Instance => this;
}
