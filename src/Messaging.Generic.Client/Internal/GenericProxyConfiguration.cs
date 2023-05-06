using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.RetryStrategies;
using Assistant.Net.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Storage single provider configured server proxy message handling.
/// </summary>
public sealed class GenericProxyConfiguration : IMessageConfiguration
{
    /// <inheritdoc/>
    public void Configure(MessagingClientBuilder builder) => builder.Services
        .AddStorage(builder.Name, b => b
            .Add<string, CachingResult>() // GenericMessagingHandlerProxy's requirement
            .Add<string, IAbstractMessage>() // GenericMessagingHandlerProxy's requirement
            .Add<string, HostsAvailabilityModel>()) // GenericMessagingHandlerProxy's requirement
        .ConfigureMessagingClientOptions(builder.Name, o => o.AddTransientException<MessageDeferredException>())
        .ConfigureGenericHandlerProxyOptions(builder.Name, o => o
            .Poll(new ConstantBackoff {MaxAttemptNumber = 10, Interval = TimeSpan.FromSeconds(0.1)})
            .UseHostSelectionStrategy(p => p.GetRequiredService<RoundRobinSelectionStrategy>()))
        .TryAddScoped<GenericMessageHandlerProxy>()
        .TryAddScoped<RoundRobinSelectionStrategy>();
}
