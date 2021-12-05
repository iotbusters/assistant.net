﻿using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
        public static IServiceCollection AddMongoMessageHandling(this IServiceCollection services, Action<MongoHandlingServerBuilder> configureBuilder) => services
            .AddHostedService<MessageHandlingService>()
            .TryAddSingleton<IMongoRecordReader, MongoRecordClient>()
            .AddScoped<IMongoRecordWriter, MongoRecordClient>()
            .AddScoped<IMongoRecordProcessor, MongoRecordProcessor>()
            .TryAddSingleton<ExceptionModelConverter>()
            .AddSystemServicesHosted()
            .AddMessagingClient(b => b.RemoveExposedException<OperationCanceledException>())
            .AddMongoClientFactory()
            .ConfigureMongoMessageHandling(configureBuilder)
            .AddOptions<MongoHandlingServerOptions>()
            .ChangeOn(MongoOptionsNames.DefaultName, typeof(MessagingClientOptions))
            .Configure<IOptionsMonitor<MessagingClientOptions>>((o, m) =>
            {
                o.MessageTypes.Clear();
                foreach (var messageType in m.Get(MongoOptionsNames.DefaultName).Handlers.Keys)
                    o.MessageTypes.Add(messageType);
            }).Services;

        /// <summary>
        ///     Configures remote message handling, required services and <see cref="MongoHandlingServerOptions"/>.
        /// </summary>
        public static IServiceCollection ConfigureMongoMessageHandling(this IServiceCollection services, Action<MongoHandlingServerBuilder> configureBuilder)
        {
            var builder = new MongoHandlingServerBuilder(services);
            configureBuilder(builder);
            return services;
        }

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
