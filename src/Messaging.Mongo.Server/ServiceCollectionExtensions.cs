using Assistant.Net.Messaging.Abstractions;
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
        public static IServiceCollection AddMongoMessageHandling(this IServiceCollection services, Action<MongoHandlingBuilder> configureBuilder) => services
            .AddHostedService<MessageHandlingService>()
            .TryAddSingleton<IMongoRecordReader, MongoRecordClient>()
            .TryAddScoped<IMongoRecordWriter, MongoRecordClient>()
            .TryAddScoped<IMongoRecordProcessor, MongoRecordProcessor>()
            .TryAddSingleton<ExceptionModelConverter>()
            .AddMessagingClient()
            .AddSystemServicesHosted()
            .AddMongoClientFactory()
            .ConfigureMongoMessageHandling(b => b.RemoveExposedException<OperationCanceledException>())
            .ConfigureMongoMessageHandling(configureBuilder)
            .AddOptions<MongoHandlingOptions>()
            .ChangeOn(MongoOptionsNames.DefaultName, typeof(MessagingClientOptions))
            .Configure<IOptionsMonitor<MessagingClientOptions>>((o, m) =>
            {
                o.MessageTypes.Clear();
                foreach (var messageType in m.Get(MongoOptionsNames.DefaultName).Handlers.Keys)
                    o.MessageTypes.Add(messageType);
            }).Services;

        /// <summary>
        ///     Configures remote message handling, required services and <see cref="MongoHandlingOptions"/>.
        /// </summary>
        public static IServiceCollection ConfigureMongoMessageHandling(this IServiceCollection services, Action<MongoHandlingBuilder> configureBuilder)
        {
            var builder = new MongoHandlingBuilder(services);
            configureBuilder(builder);
            return services;
        }

        /// <summary>
        ///    Register an action used to configure <see cref="MongoHandlingOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureMongoHandlingOptions(this IServiceCollection services, Action<MongoHandlingOptions> configureOptions) => services
            .Configure(configureOptions);

        /// <summary>
        ///    Register an action used to configure <see cref="MongoHandlingOptions"/> options.
        /// </summary>
        public static IServiceCollection ConfigureMongoHandlingOptions(this IServiceCollection services, IConfigurationSection configuration) => services
            .Configure<MongoHandlingOptions>(configuration);
    }
}
