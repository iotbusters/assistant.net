using Assistant.Net.Messaging.Internal;
using Microsoft.AspNetCore.Builder;

namespace Assistant.Net.Messaging;

/// <summary>
///     Application builder extensions for remote message handling.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    ///     Adds message handling middleware to pipeline intercepting remote message handling requests:
    ///     <see cref="DiagnosticMiddleware"/>, <see cref="ExceptionHandlingMiddleware"/> and <see cref="MessageHandlingMiddleware"/>.
    /// </summary>
    /// <remarks>
    ///     pay attention, It should be registered before routing middlewares.
    /// </remarks>
    public static IApplicationBuilder UseRemoteWebMessageHandler(this IApplicationBuilder builder) => builder
        .UseMiddleware<DiagnosticMiddleware>()
        .UseRemoteExceptionHandling()
        .UseMiddleware<MessageHandlingMiddleware>();

    /// <summary>
    ///     Adds exception handling middleware to pipeline handling occurred exceptions during remote message handling requests.
    ///     It should be registered before routing middlewares.
    /// </summary>
    /// <remarks>
    ///     Pay attention, it duplicates <see cref="UseRemoteWebMessageHandler" /> behavior.
    /// </remarks>
    public static IApplicationBuilder UseRemoteExceptionHandling(this IApplicationBuilder builder) => builder
        .UseMiddleware<ExceptionHandlingMiddleware>();
}
