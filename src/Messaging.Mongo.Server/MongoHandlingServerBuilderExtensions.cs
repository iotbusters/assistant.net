using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     MongoDB based messaging handling configuration extensions for a server.
    /// </summary>
    public static class MongoHandlingServerBuilderExtensions
    {
        /// <summary>
        ///     Configures the storage to connect a MongoDB database.
        /// </summary>
        public static MongoHandlingServerBuilder Use(this MongoHandlingServerBuilder builder, string connectionString) =>
            builder.Use(o => o.ConnectionString = connectionString);

        /// <summary>
        ///     Configures the storage to connect a MongoDB database.
        /// </summary>
        public static MongoHandlingServerBuilder Use(this MongoHandlingServerBuilder builder, Action<MongoOptions> configureOptions)
        {
            builder.Services
                .AddMongoClientFactory()
                .ConfigureMongoOptions(MongoOptions.ServerName, configureOptions);
            return builder;
        }

        /// <summary>
        ///     Configures MongoDB database connection.
        /// </summary>
        public static MongoHandlingServerBuilder Use(this MongoHandlingServerBuilder builder, IConfigurationSection configuration)
        {
            builder.Services
                .AddMongoClientFactory()
                .ConfigureMongoOptions(MongoOptions.ServerName, configuration);
            return builder;
        }

        /// <summary>
        ///     Registers MongoDB based <typeparamref name="TMessageHandler" /> type accepting remote message handling requests on a server.
        /// </summary>
        /// <typeparam name="TMessageHandler">Specific message handler type.</typeparam>
        /// <exception cref="ArgumentException"/>
        public static MongoHandlingServerBuilder AddHandler<TMessageHandler>(this MongoHandlingServerBuilder builder)
            where TMessageHandler : class => builder.AddHandler(typeof(TMessageHandler));

        /// <summary>
        ///     Registers MongoDB based <paramref name="handlerType" /> accepting remote message handling requests on a server.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MongoHandlingServerBuilder AddHandler(this MongoHandlingServerBuilder builder, Type handlerType)
        {
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

            builder.Services.ConfigureMessagingClient(MongoOptionsNames.DefaultName, b => b.AddLocalHandler(handlerType));
            return builder;
        }

        /// <summary>
        ///     Registers MongoDB based <paramref name="handlerInstance" /> accepting remote message handling requests on a server.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static MongoHandlingServerBuilder AddHandler(this MongoHandlingServerBuilder builder, object handlerInstance)
        {
            var handlerType = handlerInstance.GetType();
            if (!handlerType.IsMessageHandler())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerInstance));

            builder.Services.ConfigureMessagingClient(MongoOptionsNames.DefaultName, b => b.AddLocalHandler(handlerInstance));
            return builder;
        }

        /// <summary>
        ///     Removes all MongoDB based handlers from the list.
        /// </summary>
        public static MongoHandlingServerBuilder ClearHandlers(this MongoHandlingServerBuilder builder)
        {
            builder.Services.ConfigureMessagingClient(MongoOptionsNames.DefaultName, b => b.ClearHandlers());
            return builder;
        }
    }
}
