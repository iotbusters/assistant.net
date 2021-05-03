using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Register an action used to configure <see cref="CommandOptions"/> options.
        /// </summary>
        public static IServiceCollection AddCommandOptions(this IServiceCollection services, Action<CommandOptions> configureOptions) => services
            .AddCommandOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

        /// <summary>
        ///     Register an action used to configure the same named <see cref="CommandOptions"/> options.
        /// </summary>
        public static IServiceCollection AddCommandOptions(this IServiceCollection services, string name, Action<CommandOptions> configureOptions) => services
            .Configure(name, configureOptions);
    }
}