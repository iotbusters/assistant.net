using Assistant.Net.Messaging.Internal;
using Assistant.Net.Messaging.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Messaging;

/// <summary>
///     Application builder extensions for remote message handling.
/// </summary>
public static class ApplicationBuilderExtensions
{
    private const string OptionNameKey = "option-names";

    /// <summary>
    ///     Adds message handling middleware to pipeline intercepting remote message handling requests:
    ///     <see cref="DiagnosticMiddleware"/>, <see cref="ExceptionHandlingMiddleware"/> and <see cref="MessageHandlingMiddleware"/>.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="names">Names of related option instances.</param>
    /// <remarks>
    ///     It configures
    ///     <see cref="EndpointRoutingApplicationBuilderExtensions.UseRouting"/>,
    ///     <see cref="EndpointRoutingApplicationBuilderExtensions.UseEndpoints"/>,
    ///     and other middlewares
    ///     <see cref="DiagnosticMiddleware"/>,
    ///     <see cref="ExceptionHandlingMiddleware"/>,
    ///     <see cref="MessageHandlingMiddleware"/>,
    ///     <see cref="HealthCheckMiddleware"/>.
    /// </remarks>
    public static IApplicationBuilder UseWebMessageHandling(this IApplicationBuilder builder, params string[] names) => builder
        .UseRouting()
        .UseEndpoints(b =>
        {
            var options = b.ServiceProvider.GetRequiredService<IOptionsMonitor<WebHandlingServerOptions>>();
            b.MapPost("/messages", b
                .CreateApplicationBuilder()
                .UseMiddleware<DiagnosticMiddleware>()
                .UseMiddleware<ExceptionHandlingMiddleware>()
                .Use(b.ServiceProvider.Create<MessageHandlingMiddleware>(names, options).InvokeAsync)
                .Build());
            b.MapHealthChecks("/health", new() {AllowCachingResponses = false});
        });
}
