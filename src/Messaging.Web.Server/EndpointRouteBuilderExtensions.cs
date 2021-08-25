using Assistant.Net.Messaging.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Endpoint route builder extensions for remote message handling.
    /// </summary>
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        ///     Adds message handling endpoint to existing endpoint middleware.
        ///     Pay attention, it duplicates <see cref="ApplicationBuilderExtensions.UseRemoteWebMessageHandler" /> behavior.
        /// </summary>
        public static IEndpointConventionBuilder MapRemoteMessageHandling(this IEndpointRouteBuilder builder)
        {
            return builder.MapPost("/messages", async context =>
            {
                await ActivatorUtilities
                    .CreateInstance<RemoteMessageHandlingEndpointMiddleware>(context.RequestServices)
                    .Invoke(context);
            });
        }
    }
}