using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Storage.Configuration
{
    /// <summary>
    ///     Type marker designed to be hooked by specific storage provider's extensions methods.
    /// </summary>
    public class StorageBuilder
    {
        public StorageBuilder(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }
    }
}