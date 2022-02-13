using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Mongo.Tests.Mocks;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Assistant.Net.Messaging.Mongo.Tests.Fixtures
{
    public class MessagingClientFixtureBuilder
    {
        private readonly TestConfigureOptionsSource remoteSource = new();
        private readonly TestConfigureOptionsSource clientSource = new();

        public MessagingClientFixtureBuilder()
        {
            Services = new ServiceCollection()
                .AddMessagingClient(b => b
                    .RemoveInterceptor<CachingInterceptor>()
                    .RemoveInterceptor<RetryingInterceptor>()
                    .RemoveInterceptor<TimeoutInterceptor>())
                .ConfigureMongoHandlingClientOptions(o => o.ResponsePoll = new ConstantBackoff
                {
                    Interval = TimeSpan.FromSeconds(0.01), MaxAttemptNumber = 3
                })
                .AddOptions<MessagingClientOptions>()
                .Bind(clientSource)
                .Services;
            RemoteHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices(s => s
                    .AddMongoMessageHandling(b => b
                        .RemoveInterceptor<CachingInterceptor>()
                        .RemoveInterceptor<RetryingInterceptor>()
                        .RemoveInterceptor<TimeoutInterceptor>())
                    .ConfigureMongoHandlingServerOptions(o =>
                    {
                        o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                        o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                    })
                    .AddOptions<MessagingClientOptions>(MongoOptionsNames.DefaultName)
                    .Bind(remoteSource));
        }

        public IServiceCollection Services { get; init; }
        public IHostBuilder RemoteHostBuilder { get; init; }

        public MessagingClientFixtureBuilder UseMongo(string connectionString, string database)
        {
            Services.ConfigureMessagingClient(b => b
                .UseMongo(o => o.Connection(connectionString).Database(database))
                .TimeoutIn(TimeSpan.FromSeconds(0.5)));
            RemoteHostBuilder.ConfigureServices(s => s.ConfigureMongoMessageHandling(b => b
                .UseMongo(o => o.Connection(connectionString).Database(database))
                .TimeoutIn(TimeSpan.FromSeconds(0.5))));
            return this;
        }

        public MessagingClientFixtureBuilder AddMongoHandler<THandler>(THandler? handler = null) where THandler : class
        {
            var messageType = typeof(THandler).GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                              ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));

            remoteSource.Configurations.Add(o =>
            {
                if (handler != null)
                    o.AddHandler(handler);
                else
                    o.AddHandler(typeof(THandler));
            });
            clientSource.Configurations.Add(o => o.AddMongo(messageType));
            return this;
        }

        public MessagingClientFixtureBuilder AddMongoMessageRegistrationOnly<TMessage>()
            where TMessage : IAbstractMessage
        {
            clientSource.Configurations.Add(o =>
            {
                var messageType = typeof(TMessage);
                o.AddMongo(messageType);
            });
            return this;
        }

        public MessagingClientFixture Create()
        {
            var provider = Services.BuildServiceProvider();
            var host = RemoteHostBuilder.Start();
            return new(remoteSource, clientSource, provider, host);
        }
    }
}
