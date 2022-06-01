using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage;
using System;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Configuration with default set of interceptors:
///     <see cref="DiagnosticsInterceptor" />, <see cref="ErrorHandlingInterceptor" />,
///     <see cref="CachingInterceptor" />, <see cref="RetryingInterceptor" />,
///     <see cref="TimeoutInterceptor" />
/// </summary>
public class DefaultInterceptorConfiguration : IMessageConfiguration<MessagingClientBuilder>
{
    /// <inheritdoc/>
    public void Configure(MessagingClientBuilder builder)
    {
        builder.Services
            .AddStorage()
            .ConfigureStorage(builder.Name, b => b.AddLocal<string, CachingResult>())
            .ConfigureMessagingClient(builder.Name, o => o
                .Retry(new ExponentialBackoff {MaxAttemptNumber = 5, Interval = TimeSpan.FromSeconds(1), Rate = 1.2})
                .TimeoutIn(TimeSpan.FromSeconds(1)));
        builder
            .ClearInterceptors()
            .AddInterceptor<DiagnosticsInterceptor>()
            .AddInterceptor<CachingInterceptor>()
            .AddInterceptor<ErrorHandlingInterceptor>()
            .AddInterceptor<RetryingInterceptor>()
            .AddInterceptor<TimeoutInterceptor>()
            .ClearExposedExceptions()
            .ExposeException<TimeoutException>()
            .ExposeException<OperationCanceledException>()
            .ClearTransientExceptions()
            .AddTransientException<TimeoutException>();
    }
}
