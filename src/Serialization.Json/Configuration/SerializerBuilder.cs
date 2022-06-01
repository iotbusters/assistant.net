using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Assistant.Net.Serialization.Configuration;

/// <summary>
///     Type marker designed to be hooked by associated serializer extensions methods.
/// </summary>
public class SerializerBuilder
{
    /// <summary/>
    public SerializerBuilder(IServiceCollection services, string name)
    {
        Services = services;
        Name = name;
    }

    /// <summary/>
    public IServiceCollection Services { get; }

    /// <summary>
    ///     The name of the <see cref="JsonSerializerOptions"/> instance.
    /// </summary>
    public string Name { get; }
}
