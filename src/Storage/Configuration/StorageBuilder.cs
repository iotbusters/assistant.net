using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Storage.Configuration
{
    public class StorageBuilder
    {
        public StorageBuilder(IServiceCollection services) => Services = services;

        public IServiceCollection Services {get;}
    }
}