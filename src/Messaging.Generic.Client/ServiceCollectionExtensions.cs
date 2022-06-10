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
    ///    Register an action used to configure <see cref="GenericHandlerProxyOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureGenericHandlerProxyOptions(this IServiceCollection services, Action<GenericHandlerProxyOptions> configureOptions) => services
        .Configure(configureOptions);

    /// <summary>
    ///    Register an action used to configure <see cref="GenericHandlerProxyOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureGenericHandlerProxyOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<GenericHandlerProxyOptions>(configuration);
}
