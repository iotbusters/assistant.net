using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage;
using System;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Configuration with default set of interceptors:
///     <see cref="DiagnosticsInterceptor"/>, <see cref="ErrorHandlingInterceptor"/>,
///     <see cref="CachingInterceptor"/>, <see cref="RetryingInterceptor"/>,
///     <see cref="TimeoutInterceptor"/>
/// </summary>
public sealed class DefaultInterceptorConfiguration : IMessageConfiguration
{
    /// <inheritdoc/>
    public void Configure(MessagingClientBuilder builder)
    {
        builder.Services
            .AddStorage(builder.Name, b => b
                .AddLocal<IAbstractMessage, CachingResult>()) // CachingInterceptor's requirement
            .ConfigureMessagingClient(builder.Name, o => o
                .Retry(new ExponentialBackoff {MaxAttemptNumber = 5, Interval = TimeSpan.FromSeconds(1), Rate = 1.2})
                .TimeoutIn(TimeSpan.FromSeconds(1)));
        builder
            .AddInterceptor<DiagnosticsInterceptor>()
            .AddInterceptor<CachingInterceptor>()
            .AddInterceptor<ErrorHandlingInterceptor>()
            .AddInterceptor<CancellationDelayInterceptor>()
            .AddInterceptor<RetryingInterceptor>()
            .AddInterceptor<TimeoutInterceptor>()
            .ExposeException<TimeoutException>()
            .AddTransientException<TimeoutException>();
    }
}
