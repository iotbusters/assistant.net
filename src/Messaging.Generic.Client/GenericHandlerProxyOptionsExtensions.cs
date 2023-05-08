using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Options;
using Microsoft.Extensions.Configuration;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Generic message handler proxy options extensions.
/// </summary>
public static class GenericHandlerProxyOptionsExtensions
{
    /// <summary>
    ///     Configures response polling <paramref name="strategy"/> instance.
    /// </summary>
    public static GenericHandlerProxyOptions UseResponsePollingStrategy(this GenericHandlerProxyOptions options, IRetryStrategy strategy)
    {
        options.ResponsePoll = strategy;
        return options;
    }

    /// <summary>
    ///     Configures host selection <paramref name="strategy"/>.
    /// </summary>
    public static GenericHandlerProxyOptions UseHostSelectionStrategy(this GenericHandlerProxyOptions options, IHostSelectionStrategy strategy) => options
        .UseHostSelectionStrategy(_ => strategy);

    /// <summary>
    ///     Configures host selection <paramref name="strategyFactory"/>.
    /// </summary>
    public static GenericHandlerProxyOptions UseHostSelectionStrategy(this GenericHandlerProxyOptions options, Func<IServiceProvider, IHostSelectionStrategy> strategyFactory)
    {
        options.HostSelectionStrategyFactory = new InstanceCachingFactory<IHostSelectionStrategy>(strategyFactory);
        return options;
    }

    /// <summary>
    ///     Overrides the polling response strategy.
    /// </summary>
    public static GenericHandlerProxyOptions Poll(this GenericHandlerProxyOptions options, IRetryStrategy strategy)
    {
        options.ResponsePoll = strategy;
        return options;
    }

    /// <summary>
    ///     Overrides the polling response strategy.
    /// </summary>
    public static GenericHandlerProxyOptions Poll(this GenericHandlerProxyOptions options, IConfigurationSection configuration) => options
        .Poll(IRetryStrategy.ReadStrategy(configuration));
}
