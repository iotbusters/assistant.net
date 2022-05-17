using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for MongoDb based remote message handling on a client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///    Register an action used to configure <see cref="MongoHandlingClientOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureMongoHandlingClientOptions(this IServiceCollection services, Action<MongoHandlingClientOptions> configureOptions) => services
        .Configure(configureOptions);

    /// <summary>
    ///    Register an action used to configure <see cref="MongoHandlingClientOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureMongoHandlingClientOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .Configure<MongoHandlingClientOptions>(configuration);
}
