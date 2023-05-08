using Assistant.Net.Messaging.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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
            if (names.Length == 0)
                names = new[] {Microsoft.Extensions.Options.Options.DefaultName};
            var appBuilder = b
                .CreateApplicationBuilder()
                .UseMiddleware<DiagnosticMiddleware>()
                .UseMiddleware<ExceptionHandlingMiddleware>();

            foreach (var name in names)
                appBuilder.Use((ctx, next) => ctx.RequestServices.Create<MessageHandlingMiddleware>(name).InvokeAsync(ctx, next));

            appBuilder.Use(new MessageHandlingMiddleware().InvokeAsync); // backoff MessageNotRegisteredException scenario.

            b.MapPost("/messages", appBuilder.Build());
            b.MapHealthChecks("/health", new() {AllowCachingResponses = false});
        });
}
