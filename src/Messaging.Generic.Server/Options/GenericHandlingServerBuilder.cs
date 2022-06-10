using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Storage based server message handling builder.
/// </summary>
public class GenericHandlingServerBuilder
{
    /// <summary/>
    public GenericHandlingServerBuilder(IServiceCollection services) =>
        Services = services;

    /// <summary/>
    public IServiceCollection Services { get; }
}
