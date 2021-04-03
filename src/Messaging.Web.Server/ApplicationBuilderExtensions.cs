using Assistant.Net.Messaging.Internal;
using Microsoft.AspNetCore.Builder;

namespace Assistant.Net.Messaging
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Adds command handling middleware to pipeline intercepting related requests.
        ///     It should be registered before routing middlewares.
        ///     Pay attention, it duplicates <see cref="EndpointRouteBuilderExtensions.MapRemoteCommandHandling" /> behavior.
        /// </summary>
        public static IApplicationBuilder UseRemoteCommandHandling(this IApplicationBuilder builder) => builder
            .UseMiddleware<RemoteCommandHandlingMiddleware>();
    }
}