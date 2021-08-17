using Assistant.Net.Messaging.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Endpoint route builder extensions for remote command handling.
    /// </summary>
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        ///     Adds command handling endpoint to existing endpoint middleware.
        ///     Pay attention, it duplicates <see cref="ApplicationBuilderExtensions.UseRemoteWebCommandHandler" /> behavior.
        /// </summary>
        public static IEndpointConventionBuilder MapRemoteCommandHandling(this IEndpointRouteBuilder builder)
        {
            return builder.MapPost("/command", async context =>
            {
                await ActivatorUtilities
                    .CreateInstance<RemoteCommandHandlingEndpointMiddleware>(context.RequestServices)
                    .Invoke(context);
            });
        }
    }
}