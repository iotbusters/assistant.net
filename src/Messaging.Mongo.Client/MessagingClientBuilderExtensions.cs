﻿using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Assistant.Net.Storage;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     MongoDB based messaging client configuration extensions for a client.
/// </summary>
public static class MessagingClientBuilderExtensions
{
    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, string connectionString) =>
        builder.UseMongo(o => o.ConnectionString = connectionString);

    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, Action<MongoOptions> configureOptions)
    {
        builder.Services
            .AddStorage(b => b
                .UseMongo(o => o.Database(MongoNames.DatabaseName))
                .UseMongo(configureOptions)
                .AddMongoPartitioned<int, IAbstractMessage>()
                .AddMongo<int, CachingResult>());
        return builder;
    }

    /// <summary>
    ///     Configures the messaging client to connect a MongoDB database from a client.
    /// </summary>
    public static MessagingClientBuilder UseMongo(this MessagingClientBuilder builder, IConfigurationSection configuration)
    {
        builder.Services
            .AddStorage(b => b
                .UseMongo(o => o.Database(MongoNames.DatabaseName))
                .UseMongo(configuration)
                .AddMongoPartitioned<int, IAbstractMessage>()
                .AddMongo<int, CachingResult>());
        return builder;
    }

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

        builder.Services.ConfigureMessagingClientOptions(builder.Name, o => o.AddMongo(messageType));
        return builder;
    }
}
