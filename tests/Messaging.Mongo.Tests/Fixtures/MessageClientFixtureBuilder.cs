using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.RetryStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Assistant.Net.Messaging.Mongo.Tests.Fixtures
{
    public class MessageClientFixtureBuilder
    {
        public MessageClientFixtureBuilder()
        {
            Services = new ServiceCollection()
                .AddMessagingClient(b => b.RemoveInterceptor<CachingInterceptor>().RemoveInterceptor<RetryingInterceptor>());
            RemoteHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureServices(s => s
                    .AddMongoMessageHandler(_ => { })
                    .ConfigureMessagingClient(b => b.RemoveInterceptor<CachingInterceptor>().RemoveInterceptor<RetryingInterceptor>()));
        }

        public IServiceCollection Services { get; init; }
        public IHostBuilder RemoteHostBuilder { get; init; }

        public MessageClientFixtureBuilder UseMongo(string connectionString, string database)
        {
            Services
                .ConfigureMessagingClient(b => b.UseMongo(o => o.ConnectionString = connectionString))
                .ConfigureMongoHandlingClientOptions(o =>
                {
                    o.DatabaseName = database;
                    o.ResponsePoll = new ConstantBackoff {Interval = TimeSpan.FromSeconds(0.01), MaxAttemptNumber = 100};
                });
            RemoteHostBuilder.ConfigureServices(s => s
                .ConfigureMongoOptions(o => o.ConnectionString = connectionString)
                .ConfigureMongoHandlingServerOptions(o =>
                {
                    o.DatabaseName = database;
                    o.InactivityDelayTime = TimeSpan.FromSeconds(0.001);
                    o.NextMessageDelayTime = TimeSpan.FromSeconds(0.001);
                }));
            return this;
        }

        public MessageClientFixtureBuilder ClearHandlers()
        {
            Services.ConfigureMessagingClient(b => b.ClearInterceptors());
            return this;
        }

        public MessageClientFixtureBuilder AddLocal<THandler>() where THandler : class, IAbstractHandler
        {
            Services.ConfigureMessagingClient(b => b.AddLocal<THandler>());
            return this;
        }

        public MessageClientFixtureBuilder AddMongo<THandler>(THandler? instance = null) where THandler : class, IAbstractHandler
        {
            RemoteHostBuilder.ConfigureServices(s => s.ConfigureMessagingClient(b =>
            {
                if (instance != null)
                    b.AddLocal(instance);
                else
                    b.AddLocal<THandler>();
            }));

            var messageType = typeof(THandler)
                .GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IMessageHandler<,>))
                ?.GetGenericArguments().First()
                ?? throw new ArgumentException("Invalid message handler type.", nameof(THandler));

            Services.ConfigureMessagingClient(b => b.AddMongo(messageType));
            return this;
        }

        public MessageClientFixtureBuilder AddMongoMessageRegistrationOnly<TMessage>()
            where TMessage : IAbstractMessage
        {
            var messageType = typeof(TMessage);
            Services.ConfigureMessagingClient(b => b.AddMongo(messageType));
            return this;
        }

        public MessageClientFixture Create()
        {
            var provider = Services.BuildServiceProvider();
            var host = RemoteHostBuilder.Start();
            return new(provider, host);
        }
    }
}
