using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Serialization.Configuration
{
    public class SerializerBuilder
    {
        public SerializerBuilder(IServiceCollection services) =>
            Services = services;

        internal IServiceCollection Services{get;}
    }
}