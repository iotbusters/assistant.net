using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging.Options
{
    public class CommandClientBuilder
    {
        public CommandClientBuilder(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }
    }
}