using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Extensions;
using Assistant.Net.Messaging.Internal;
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
    ///     Registers empty remote messaging handling configuration.
    /// </summary>
    public static IHttpClientBuilder AddRemoteWebMessagingClient(this IServiceCollection services) => services
        .AddSystemLifetime()
        .AddDiagnostics()
        .AddTypeEncoder()
        .AddHttpClient<IWebMessageHandlerClient, WebMessageHandlerClient>(c => c.Timeout = DefaultTimeout)
        .AddHttpMessageHandler<CorrelationHandler>()
        .AddHttpMessageHandler<OperationHandler>()
        .AddHttpMessageHandler<AuthorizationHandler>()
        .AddHttpMessageHandler<ErrorPropagationHandler>();
}
