using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Serialization.Configuration;

/// <summary>
///     Type marker designed to be hooked by associated serializer extensions methods.
/// </summary>
public class SerializerBuilder
{
    /// <summary/>
    public SerializerBuilder(IServiceCollection services) =>
        Services = services;

    internal IServiceCollection Services{get;}
}