using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage;
using System;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Storage single provider configured server message handling.
/// </summary>
public sealed class GenericServerInterceptorConfiguration : IMessageConfiguration
{
    /// <inheritdoc/>
    public void Configure(MessagingClientBuilder builder)
    {
        builder
            .AddConfiguration<DefaultInterceptorConfiguration>()
            .ReplaceInterceptor<CachingInterceptor, RespondingInterceptor>()
            .Retry(new ExponentialBackoff {MaxAttemptNumber = 5, Interval = TimeSpan.FromSeconds(1), Rate = 1.2})
            .TimeoutIn(TimeSpan.FromSeconds(3));
        builder.Services.AddStorage(builder.Name, b => b
            .AddSingle<IAbstractMessage, CachingResult>()
            .AddSinglePartitioned<int, IAbstractMessage>()
            .AddSingle<int, long>());
    }
}
