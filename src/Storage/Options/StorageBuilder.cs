using Assistant.Net.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Storage.Options;

/// <summary>
///     Type marker designed to be hooked by specific storage provider's extensions methods.
/// </summary>
public class StorageBuilder : IBuilder<StorageBuilder>
{
    /// <summary/>
    public StorageBuilder(IServiceCollection services, string name)
    {
        Services = services;
        Name = name;
    }

    /// <summary/>
    public IServiceCollection Services { get; }

    /// <inheritdoc/>
    public StorageBuilder Instance => this;

    /// <summary>
    ///     The name of the <see cref="StorageOptions"/> instance.
    /// </summary>
    public string Name { get; }
}
