using Assistant.Net.Messaging.Internal;
using Microsoft.AspNetCore.Builder;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Application builder extensions for remote message handling.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Adds message handling middleware to pipeline intercepting remote message handling requests.
        ///     It should be registered before routing middlewares.
        /// </summary>
        public static IApplicationBuilder UseRemoteWebMessageHandler(this IApplicationBuilder builder) => builder
            .UseRemoteExceptionHandling()
            .UseMiddleware<RemoteDiagnosticMiddleware>()
            .UseMiddleware<RemoteMessageHandlingMiddleware>();

        /// <summary>
        ///     Adds exception handling middleware to pipeline handling occurred exceptions during remote message handling requests.
        ///     It should be registered before routing middlewares.
        /// </summary>
        /// <remarks>
        ///     Pay attention, it duplicates <see cref="UseRemoteWebMessageHandler" /> behavior.
        /// </remarks>
        public static IApplicationBuilder UseRemoteExceptionHandling(this IApplicationBuilder builder) => builder
            .UseMiddleware<RemoteExceptionHandlingMiddleware>();
    }
}
