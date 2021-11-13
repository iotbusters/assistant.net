using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

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

            var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First());
            builder.Services
                .ConfigureMessagingClient(b => b.AddLocalHandler(handlerType))
                .ConfigureMongoHandlingServerOptions(o => o.MessageTypes.AddRange(messageTypes));
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

            var messageTypes = handlerType.GetMessageHandlerInterfaceTypes().Select(x => x.GetGenericArguments().First());
            builder.Services
                .ConfigureMessagingClient(b => b.AddLocalHandler(handlerInstance))
                .ConfigureMongoHandlingServerOptions(o => o.MessageTypes.AddRange(messageTypes));
            return builder;
        }

        /// <summary>
        ///     Removes all handlers from the list.
        /// </summary>
        public static MongoHandlingServerBuilder ClearHandlers(this MongoHandlingServerBuilder builder)
        {
            builder.Services
                .ConfigureMessagingClient(b => b.ClearHandlers())
                .ConfigureMongoHandlingServerOptions(o => o.MessageTypes.Clear());
            return builder;
        }

        /// <summary>
        ///     Removes an handler of <typeparamref name="TMessage" /> from the list.
        /// </summary>
        public static MongoHandlingServerBuilder Remove<TMessage>(this MongoHandlingServerBuilder builder)
            where TMessage : class => builder.Remove(typeof(TMessage));

        /// <summary>
        ///     Removes an handler of <paramref name="messageType" /> from the list.
        /// </summary>
        public static MongoHandlingServerBuilder Remove(this MongoHandlingServerBuilder builder, Type messageType)
        {
            if(!messageType.IsMessage())
                throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

            builder.Services
                .ConfigureMessagingClient(b => b.Remove(messageType))
                .ConfigureMongoHandlingServerOptions(o => o.MessageTypes.Remove(messageType));
            return builder;
        }

        /// <summary>
        ///     Removes the handler type <typeparamref name="THandler"/> from the list.
        /// </summary>
        public static MongoHandlingServerBuilder RemoveHandler<THandler>(this MongoHandlingServerBuilder builder)
            where THandler : class => builder.RemoveHandler(typeof(THandler));

        /// <summary>
        ///     Removes the <paramref name="handlerType" /> from the list.
        /// </summary>
        public static MongoHandlingServerBuilder RemoveHandler(this MongoHandlingServerBuilder builder, Type handlerType)
        {
            var handlerInterfaceTypes = handlerType.GetMessageHandlerInterfaceTypes();
            if (!handlerInterfaceTypes.Any())
                throw new ArgumentException($"Expected message handler but provided {handlerType}.", nameof(handlerType));

            builder.Services
                .ConfigureMessagingClient(b => b.RemoveHandler(handlerType))
                .ConfigureMongoHandlingServerOptions(o =>
                {
                    var messageTypes = handlerInterfaceTypes.Select(x => x.GetGenericArguments().First());
                    foreach (var messageType in messageTypes)
                        o.MessageTypes.Remove(messageType);
                });
            return builder;
        }
    }
}
