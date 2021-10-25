using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Assistant.Net.Messaging.Mongo.Tests.Fixtures
{
    public class MessagingClientFixtureBuilder
    {
        public MessagingClientFixtureBuilder()
        {
            Services = new ServiceCollection()
                .AddMessagingClient(b => b.RemoveInterceptor<CachingInterceptor>().RemoveInterceptor<RetryingInterceptor>())
                .ConfigureMongoHandlingClientOptions(o => o.ResponsePoll = new ConstantBackoff
                {
                    Interval = TimeSpan.FromSeconds(0.01), MaxAttemptNumber = 3
                });
            RemoteHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices(s => s
                    .AddMongoMessageHandling(_ => { })
                    .ConfigureMessagingClient(b => b.RemoveInterceptor<CachingInterceptor>().RemoveInterceptor<RetryingInterceptor>())
                    .ConfigureMongoHandlingServerOptions(o =>
                    {
                        o.InactivityDelayTime = TimeSpan.FromSeconds(0.005);
                        o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                    }));
        }

        public IServiceCollection Services { get; init; }
        public IHostBuilder RemoteHostBuilder { get; init; }

        public MessagingClientFixtureBuilder UseMongo(string connectionString, string database)
        {
            Services
                .ConfigureMessagingClient(b => b
                    .UseMongo(o => o.ConnectionString = connectionString)
                    .TimeoutIn(TimeSpan.FromSeconds(0.5)))
                .ConfigureMongoHandlingClientOptions(o => o.DatabaseName = database);
            RemoteHostBuilder.ConfigureServices(s => s
                .ConfigureMongoMessageHandling(b => b.Use(o => o.ConnectionString = connectionString))
                .ConfigureMongoHandlingServerOptions(o => o.DatabaseName = database));
            return this;
        }

        public MessagingClientFixtureBuilder ClearHandlers()
        {
            Services.ConfigureMessagingClient(b => b.ClearInterceptors());
            return this;
        }

        public MessagingClientFixtureBuilder AddLocalHandler<THandler>() where THandler : class, IAbstractHandler
        {
            Services.ConfigureMessagingClient(b => b.AddLocalHandler<THandler>());
            return this;
        }

        public MessagingClientFixtureBuilder AddMongoHandler<THandler>(THandler? instance = null) where THandler : class, IAbstractHandler
        {
            RemoteHostBuilder.ConfigureServices(s => s.ConfigureMongoMessageHandling(b =>
            {
                if (instance != null)
                    b.AddHandler(instance);
                else
                    b.AddHandler<THandler>();
            }));

            var messageType = typeof(THandler).GetMessageHandlerInterfaceTypes().FirstOrDefault()?.GetGenericArguments().First()
                              ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));

            Services.ConfigureMessagingClient(b => b.AddMongo(messageType));
            return this;
        }

        public MessagingClientFixtureBuilder AddMongoMessageRegistrationOnly<TMessage>()
            where TMessage : IAbstractMessage
        {
            var messageType = typeof(TMessage);
            Services.ConfigureMessagingClient(b => b.AddMongo(messageType));
            return this;
        }

        public MessagingClientFixture Create()
        {
            var provider = Services.BuildServiceProvider();
            var host = RemoteHostBuilder.Start();
            return new(provider, host);
        }
    }
}
