using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage;
using System;

namespace Assistant.Net.Messaging.Interceptors;

/// <inheritdoc cref="DefaultInterceptorConfiguration"/>
public class SqliteServerInterceptorConfiguration : IMessageConfiguration
{
    /// <inheritdoc/>
    public void Configure(MessagingClientBuilder builder) =>
        builder.Services
            .ConfigureMessagingClient(builder.Name, b => b
                .AddConfiguration<DefaultInterceptorConfiguration>()
                .Retry(new ExponentialBackoff {MaxAttemptNumber = 5, Interval = TimeSpan.FromSeconds(1), Rate = 1.2})
                .TimeoutIn(TimeSpan.FromSeconds(3)))
            .ConfigureStorage(builder.Name, b => b.AddSqlite<string, CachingResult>());
}
