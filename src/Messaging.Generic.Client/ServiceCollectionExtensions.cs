using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for storage based remote message handling client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///    Register an action used to configure default <see cref="GenericHandlerProxyOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureGenericHandlerProxyOptions(this IServiceCollection services, Action<GenericHandlerProxyOptions> configureOptions) => services
        .ConfigureGenericHandlerProxyOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///    Register an action used to configure the same named <see cref="GenericHandlerProxyOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureGenericHandlerProxyOptions(this IServiceCollection services, string name, Action<GenericHandlerProxyOptions> configureOptions) => services
        .Configure(name, configureOptions);

    /// <summary>
    ///    Register an action used to configure default <see cref="GenericHandlerProxyOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configuration">The configuration being bound.</param>
    public static IServiceCollection ConfigureGenericHandlerProxyOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .ConfigureGenericHandlerProxyOptions(Microsoft.Extensions.Options.Options.DefaultName, configuration);

    /// <summary>
    ///    Register an action used to configure the same named <see cref="GenericHandlerProxyOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configuration">The configuration being bound.</param>
    public static IServiceCollection ConfigureGenericHandlerProxyOptions(this IServiceCollection services, string name, IConfigurationSection configuration) => services
        .Configure<GenericHandlerProxyOptions>(name, configuration);
}
