using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Service collection extensions for MongoDb based remote message handling on a server.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds system services with self-hosted service based behavior.
        /// </summary>
        public static IServiceCollection AddSystemServicesHosted(this IServiceCollection services) => services
            .AddSystemServicesDefaulted()
            .AddSystemLifetime(p => p.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

        /// <summary>
        ///     Registers remote message handling server configuration.
        /// </summary>
        /// <remarks>
        ///     Pay attention, you need to call explicitly 'ConfigureMessageClient' to register handlers.
        /// </remarks>
        public static IServiceCollection AddMongoMessageHandler(this IServiceCollection services, Action<MongoOptions> configureOptions) => services
            .AddHostedService<MessageHandlingService>()
            .AddSingleton<IMongoRecordReader, MongoRecordClient>()
            .AddScoped<IMongoRecordWriter, MongoRecordClient>()
            .AddScoped<IMongoRecordProcessor, MongoRecordProcessor>()
            .TryAddSingleton<ExceptionModelConverter>()
            .AddSystemServicesHosted()
            .AddDiagnosticContext<InternalDiagnosticContext>()
            .AddMessagingClient(b => b.RemoveExposedException<OperationCanceledException>())
            .AddMongoClient()
            .ConfigureMongoOptions(configureOptions)
            .ConfigureMongoHandlingServerOptions(_ => { });

        /// <summary>
        ///     Registers remote message handling server configuration.
        /// </summary>
        /// <remarks>
        ///     Pay attention, you need to call explicitly 'ConfigureMessageClient' to register handlers.
        /// </remarks>
        public static IServiceCollection AddMongoMessageHandler(this IServiceCollection services, IConfigurationSection configuration) => services
            .AddHostedService<MessageHandlingService>()
            .AddSystemServicesHosted()
            .AddDiagnostics()
            .AddMessagingClient(b => b.UseMongo(configuration));

        /// <summary>
        ///    Register an action used to configure <see cref="MongoHandlingServerOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureMongoHandlingServerOptions(this IServiceCollection services, Action<MongoHandlingServerOptions> configureOptions) => services
            .Configure(configureOptions);

        /// <summary>
        ///    Register an action used to configure <see cref="MongoHandlingServerOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureMongoHandlingServerOptions(this IServiceCollection services, IConfigurationSection configuration) => services
            .Configure<MongoHandlingServerOptions>(configuration);
    }
}
