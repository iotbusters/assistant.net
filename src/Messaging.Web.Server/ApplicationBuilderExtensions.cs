using Microsoft.AspNetCore.Builder;
using Assistant.Net.Messaging.Internal;

namespace Assistant.Net.Messaging
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Adds command handling middleware to pipeline intercepting remote command handling requests.
        ///     It should be registered before routing middlewares.
        ///     Pay attention, it duplicates <see cref="EndpointRouteBuilderExtensions.MapRemoteCommandHandling" /> behavior.
        /// </summary>
        public static IApplicationBuilder UseRemoteCommandHandling(this IApplicationBuilder builder) => builder
            .UseRemoteExceptionHandling()
            .UseMiddleware<DiagnosticMiddleware>()
            .UseMiddleware<RemoteCommandHandlingMiddleware>();

        /// <summary>
        ///     Adds exception handling middleware to pipeline handling occurred exceptions during remote command handling requests.
        ///     It should be registered before routing middlewares.
        ///     Pay attention, it duplicates <see cref="UseRemoteCommandHandling" /> behavior.
        /// </summary>
        public static IApplicationBuilder UseRemoteExceptionHandling(this IApplicationBuilder builder) => builder
            .UseMiddleware<RemoteExceptionHandlingMiddleware>();
    }
}