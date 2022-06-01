using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     MongoDB based messaging client configuration extensions for a client.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures the messaging client to use a MongoDB single provider implementation.
    /// </summary>
    /// <remarks>
    ///     Pay attention, you need to call explicitly one of overloaded <see cref="UseMongo(MessagingClientBuilder,string)"/> to configure.
    /// </remarks>
    public static MessagingClientBuilder UseMongoSingleProvider(this MessagingClientBuilder builder, Action<MongoOptions> configureOptions)
    {
        builder.Services
            .ConfigureMessagingClientOptions(builder.Name, o => o.UseMongoSingleProvider())
            .ConfigureStorage(builder.Name, b => b.UseMongoSingleProvider());
        return builder.AddMongoSingleProvider(b => b.UseMongo(configureOptions));
    }

    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, string connectionString) =>
        builder.UseMongo(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, Action<MongoOptions> configureOptions) => builder
        .AddMongoProvider(b => b.UseMongo(configureOptions));

    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, IConfigurationSection configuration) => builder
        .AddMongoProvider(b => b.UseMongo(configuration));

    /// <summary>
    ///     Registers remote MongoDB based handler of <typeparamref name="TMessage" /> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of UseMongo method.
    /// </remarks>
    /// <typeparam name="TMessage">Specific message type to be handled remotely.</typeparam>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddMongo<TMessage>(this MessagingClientBuilder builder)
        where TMessage : class, IAbstractMessage => builder.AddMongo(typeof(TMessage));

    /// <summary>
    ///     Registers remote MongoDB based handler of <paramref name="messageType" /> from a client.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it requires calling one of UseMongo method.
    /// </remarks>
    /// <exception cref="ArgumentException"/>
    public static MessagingClientBuilder AddMongo(this MessagingClientBuilder builder, Type messageType)
    {
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        builder.Services
            .AddMongoProvider(builder.Name)
            .ConfigureMessagingClientOptions(builder.Name, o => o.AddMongo(messageType));
        return builder;
    }

    /// <summary>
    ///     Registers MongoDB single provider and its dependencies.
    /// </summary>
    private static MessagingClientBuilder AddMongoSingleProvider(this MessagingClientBuilder builder, Action<StorageBuilder> configureBuilder)
    {
        builder.Services
            .TryAddScoped<MongoMessageHandlerProxy>()
            .AddStorage()
            .ConfigureStorage(builder.Name, b => b
                .UseMongo(o => o.Database(MongoNames.DatabaseName))
                .UseMongoSingleProvider()
                .AddPartitioned<int, IAbstractMessage>()
                .Add<int, CachingResult>())
            .ConfigureStorage(builder.Name, configureBuilder);
        return builder;
    }

    /// <summary>
    ///     Registers MongoDB provider and its dependencies.
    /// </summary>
    private static MessagingClientBuilder AddMongoProvider(this MessagingClientBuilder builder, Action<StorageBuilder> configureBuilder)
    {
        builder.Services
            .AddMongoProvider(builder.Name)
            .ConfigureStorage(builder.Name, configureBuilder);
        return builder;
    }

    /// <summary>
    ///     Registers MongoDB provider and its dependencies.
    /// </summary>
    private static IServiceCollection AddMongoProvider(this IServiceCollection services, string name)
    {
        return services
            .TryAddScoped<MongoMessageHandlerProxy>()
            .AddStorage()
            .ConfigureStorage(name, b => b
                .UseMongo(o => o.Database(MongoNames.DatabaseName))
                .AddMongoPartitioned<int, IAbstractMessage>()
                .AddMongo<int, CachingResult>());
    }
}
