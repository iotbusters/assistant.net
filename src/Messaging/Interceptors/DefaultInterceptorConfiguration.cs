using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
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
        builder.Services.AddStorage(builder.Name, b => b.Add<IAbstractMessage, CachingResult>());
        builder
            .AddInterceptor<DiagnosticsInterceptor>()
            .AddInterceptor<CachingInterceptor>()
            .AddInterceptor<ErrorHandlingInterceptor>()
            .AddInterceptor<RetryingInterceptor>()
            .AddInterceptor<TimeoutInterceptor>()
            .ExposeException<TimeoutException>()
            .ExposeException<MessageException>()
            .AddTransientException<TimeoutException>()
            .Retry(new ExponentialBackoff { MaxAttemptNumber = 3, Interval = TimeSpan.FromSeconds(0.1), Rate = 2 })
            .TimeoutIn(TimeSpan.FromSeconds(1));
    }
}
