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
            .RemoveInterceptor<CachingInterceptor>() // GenericMessageHandlingService's responsibility here
            .Retry(new ExponentialBackoff {MaxAttemptNumber = 5, Interval = TimeSpan.FromSeconds(1), Rate = 1.2})
            .TimeoutIn(TimeSpan.FromSeconds(3));
        builder.Services.AddStorage(builder.Name, b => b
            .AddSinglePartitioned<string, IAbstractMessage>() // GenericMessageHandlingService's requirement
            .AddSingle<string, CachingResult>() // GenericMessageHandlingService's requirement
            .AddSingle<string, long>() // GenericMessageHandlingService's requirement
            .AddSingle<string, RemoteHandlerModel>());// GenericServerAvailabilityPublisher's requirement
    }
}
