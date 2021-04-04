using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Internal;
using static Assistant.Net.Messaging.Options.CommandOptions;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSystemServicesHosted(this IServiceCollection services) => services
            .AddSystemClock(p => p.GetRequiredService<ISystemClock>().UtcNow)
            .AddSystemLifetime(p => p.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

        public static IServiceCollection AddRemoteCommandHandlingServer(this IServiceCollection services, Action<Configuration.CommandOptions> configureOptions) => services
            .AddJsonSerializerOptions()
            .AddCommandOptions(RemoteName, configureOptions);
    }
}