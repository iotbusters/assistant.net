using Assistant.Net.Diagnostics;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Assistant.Net.Messaging;

/// <summary>
///     Service collection extensions for remote WEB message handling on a server.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers WEB message handling server configuration.
    /// </summary>
    public static IServiceCollection AddWebMessageHandling(this IServiceCollection services) => services
        .AddWebMessageHandling(Microsoft.Extensions.Options.Options.DefaultName);

    /// <summary>
    ///     Registers WEB message handling server configuration.
    /// </summary>
    public static IServiceCollection AddWebMessageHandling(this IServiceCollection services, string name) => services
        .AddWebMessageHandlingMiddlewares()
        .AddSystemServicesHosted()
        .AddDiagnosticsWebHosted()
        .AddMessagingClient(name, b => b.AddConfiguration<WebServerInterceptorConfiguration>())
        .ConfigureJsonSerialization(name)
        .AddRouting()
        .AddHealthChecks()
        .Services;

    /// <summary>
    ///     Registers WEB message handling server configuration.
    /// </summary>
    public static IServiceCollection AddWebMessageHandling(this IServiceCollection services, Action<WebHandlingServerBuilder> configureBuilder) => services
        .AddWebMessageHandling(Microsoft.Extensions.Options.Options.DefaultName, configureBuilder);

    /// <summary>
    ///     Registers WEB message handling server configuration.
    /// </summary>
    public static IServiceCollection AddWebMessageHandling(this IServiceCollection services, string name, Action<WebHandlingServerBuilder> configureBuilder) => services
        .AddWebMessageHandling(name)
        .ConfigureWebMessageHandling(name, configureBuilder);

    /// <summary>
    ///     Configures remote message handling, required services and <see cref="WebHandlingServerOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureWebMessageHandling(this IServiceCollection services, Action<WebHandlingServerBuilder> configureBuilder) => services
        .ConfigureWebMessageHandling(Microsoft.Extensions.Options.Options.DefaultName, configureBuilder);

    /// <summary>
    ///     Configures remote message handling, required services and <see cref="WebHandlingServerOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureWebMessageHandling(this IServiceCollection services, string name, Action<WebHandlingServerBuilder> configureBuilder)
    {
        var builder = new WebHandlingServerBuilder(services, name);
        configureBuilder(builder);
        return services;
    }

    /// <summary>
    ///    Register an action used to configure <see cref="WebHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureWebHandlingServerOptions(this IServiceCollection services, Action<WebHandlingServerOptions> configureOptions) => services
        .ConfigureWebHandlingServerOptions(Microsoft.Extensions.Options.Options.DefaultName, configureOptions);

    /// <summary>
    ///    Register an action used to configure <see cref="WebHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureWebHandlingServerOptions(this IServiceCollection services, string name, Action<WebHandlingServerOptions> configureOptions) => services
        .Configure(name, configureOptions);

    /// <summary>
    ///    Register an action used to configure <see cref="WebHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureWebHandlingServerOptions(this IServiceCollection services, IConfigurationSection configuration) => services
        .ConfigureWebHandlingServerOptions(Microsoft.Extensions.Options.Options.DefaultName, configuration);

    /// <summary>
    ///    Register an action used to configure <see cref="WebHandlingServerOptions"/> options.
    /// </summary>
    public static IServiceCollection ConfigureWebHandlingServerOptions(this IServiceCollection services, string name, IConfigurationSection configuration) => services
        .Configure<WebHandlingServerOptions>(name, configuration);

    /// <summary>
    ///     Adds system services with self-hosted service based behavior.
    /// </summary>
    public static IServiceCollection AddSystemServicesHosted(this IServiceCollection services) => services
        .AddSystemServicesDefaulted()
        .AddSystemLifetime(p => p.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

    /// <summary>
    ///     Registers diagnostic services with self-hosted WEB service based behavior.
    /// </summary>
    public static IServiceCollection AddDiagnosticsWebHosted(this IServiceCollection services) => services
        .AddHttpContextAccessor()
        .AddDiagnosticContext(InitializeFromHttpContext)
        .AddDiagnostics();

    /// <summary>
    ///     Registers WEB message handling middlewares:
    ///     <see cref="DiagnosticMiddleware"/>, <see cref="ExceptionHandlingMiddleware"/> and <see cref="MessageHandlingMiddleware"/>.
    /// </summary>
    public static IServiceCollection AddWebMessageHandlingMiddlewares(this IServiceCollection services) => services
        .TryAddTransient<DiagnosticMiddleware>()
        .TryAddTransient<ExceptionHandlingMiddleware>();

    /// <exception cref="InvalidOperationException"/>
    private static void InitializeFromHttpContext(IServiceProvider provider, DiagnosticContext diagnosticContext)
    {
        var accessor = provider.GetRequiredService<IHttpContextAccessor>();
        var httpContext = accessor.HttpContext ?? throw new InvalidOperationException("HttpContext wasn't yet initialized.");

        diagnosticContext.CorrelationId = httpContext.GetCorrelationId();
        diagnosticContext.User = httpContext.User.Identity?.Name;
    }
}
