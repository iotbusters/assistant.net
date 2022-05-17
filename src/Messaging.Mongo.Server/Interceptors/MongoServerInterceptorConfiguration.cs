using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
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
        builder.Services.AddStorage(b => b.AddMongo<string, CachingResult>());
        builder
            .ClearInterceptors()
            .AddInterceptor<DiagnosticsInterceptor>()
            .AddInterceptor<MessageResponseStoringInterceptor>()
            .AddInterceptor<ErrorHandlingInterceptor>()
            .AddInterceptor<TimeoutInterceptor>()
            .ClearExposedExceptions()
            .ExposeException<TimeoutException>()
            .ClearTransientExceptions();
    }
}