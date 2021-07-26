using Assistant.Net.Dynamics.Internal;
using Assistant.Net.Dynamics.Options;
using Assistant.Net.Dynamics.Proxy.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Dynamics.Proxy
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers all proxy type implementations from all loaded assemblies.
        ///     This logic performs the role of module initializer if one of extensions was references.
        /// </summary>
        static ServiceCollectionExtensions()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (_, a) => KnownProxy.RegisterFrom(a.LoadedAssembly);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                KnownProxy.RegisterFrom(assembly);
        }

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