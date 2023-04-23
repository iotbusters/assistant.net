using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Extensions;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for remote WEB messaging handling client.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(10);

    /// <summary>
    ///     Registers WEB messaging handling client.
    /// </summary>
    public static IHttpClientBuilder AddWebMessageHandlerClient(this IServiceCollection services) => services
        .AddSystemLifetime()
        .AddDiagnostics()
        .AddTypeEncoder()
        .AddHttpClient<IWebMessageHandlerClient, WebMessageHandlerClient>((p, c) =>
        {
            c.Timeout = DefaultTimeout;

            var options = p.GetRequiredService<INamedOptions<WebHandlerProxyOptions>>();
            foreach (var configure in options.Value.Configurations)
                configure(c);
        })
        .AddHttpMessageHandler<CorrelationHandler>()
        .AddHttpMessageHandler<OperationHandler>()
        .AddHttpMessageHandler<AuthorizationHandler>()
        .AddHttpMessageHandler<ErrorPropagationHandler>();

    /// <summary>
    ///     Register an action used to configure default <see cref="WebHandlerProxyOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureWebHandlerProxyOptions(this IServiceCollection services, Action<WebHandlerProxyOptions> configureOptions) => services
        .ConfigureWebHandlerProxyOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///     Register an action used to configure the same named <see cref="WebHandlerProxyOptions"/> options.
    /// </summary>
    /// <param name="services"/>
    /// <param name="name">The name of the options instance.</param>
    /// <param name="configureOptions">The action used to configure the options.</param>
    public static IServiceCollection ConfigureWebHandlerProxyOptions(this IServiceCollection services, string name, Action<WebHandlerProxyOptions> configureOptions) => services
        .Configure(name, configureOptions);
}
