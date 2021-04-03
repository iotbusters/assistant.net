using System;
using Assistant.Net.Messaging.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRemoteCommandHandling(this IServiceCollection services, Action<CommandOptions> configureOptions) => services
            .AddJsonSerializerOptions()
            .AddCommandOptions("Remote", configureOptions);
    }
}