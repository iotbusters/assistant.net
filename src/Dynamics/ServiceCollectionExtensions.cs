using System;
using Assistant.Net.Dynamics.Internal;
using Assistant.Net.Dynamics.Options;
using Assistant.Net.Dynamics.ProxyAnalyzer.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Dynamics
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds <see cref="IProxyFactory" /> implementation.
        ///     Pay attention, you may need to call explicitly <see cref="ConfigureProxyFactoryOptions" />
        ///     to ensure required proxy types are registered.
        /// </summary>
        public static IServiceCollection AddProxyFactory(this IServiceCollection services) => services
            .TryAddSingleton<IProxyFactory, ProxyFactory>();

        /// <summary>
        ///     Adds <see cref="IProxyFactory"/> implementation and <see cref="ProxyFactoryOptions"/> configuration.
        /// </summary>
        public static IServiceCollection AddProxyFactory(this IServiceCollection services, Action<ProxyFactoryOptions> configureOptions) => services
            .ConfigureProxyFactoryOptions(configureOptions)
            .AddProxyFactory();

        /// <summary>
        ///     Register an action used to configure <see cref="ProxyFactoryOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureProxyFactoryOptions(this IServiceCollection services, Action<ProxyFactoryOptions> configureOptions) => services
            .Configure(configureOptions);
    }
}