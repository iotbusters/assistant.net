using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Exceptions;
using System;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Configuration with default set of server interceptors:
///     <see cref="DiagnosticsInterceptor" />, <see cref="ErrorHandlingInterceptor" />,
///     <see cref="MessageResponseStoringInterceptor" />, <see cref="RetryingInterceptor" />,
///     <see cref="TimeoutInterceptor" />
/// </summary>
public class MongoServerInterceptorConfiguration : IMessageConfiguration<MongoHandlingBuilder>
{
    /// <inheritdoc/>
    public void Configure(MongoHandlingBuilder builder)
    {
        builder.Services
            .AddStorage(b => b.AddMongo<string, CachingResult>())
            .ConfigureMessagingClient(builder.Name, o => o
                .Retry(new ExponentialBackoff {MaxAttemptNumber = 5, Interval = TimeSpan.FromSeconds(1), Rate = 1.2})
                .TimeoutIn(TimeSpan.FromSeconds(3)));
        builder
            .ClearInterceptors()
            .AddInterceptor<DiagnosticsInterceptor>()
            .AddInterceptor<MessageResponseStoringInterceptor>()
            .AddInterceptor<ErrorHandlingInterceptor>()
            .AddInterceptor<RetryingInterceptor>()
            .AddInterceptor<TimeoutInterceptor>()
            .ClearExposedExceptions()
            .ExposeException<TimeoutException>()
            .ClearTransientExceptions()
            .AddTransientException<StorageConcurrencyException>();
    }
}
