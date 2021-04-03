using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Messaging.Internal;

namespace Assistant.Net.Messaging
{
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        ///     Adds command handling endpoint to existing endpoint middleware.
        ///     Pay attention, it duplicates <see cref="ApplicationBuilderExtensions.UseRemoteCommandHandling" /> behavior.
        /// </summary>
        public static IEndpointConventionBuilder MapRemoteCommandHandling(this IEndpointRouteBuilder builder)
        {
            return builder.MapPost("/command/{commandName}", async context =>
            {
                await ActivatorUtilities
                    .CreateInstance<RemoteCommandHandlingEndpointMiddleware>(context.RequestServices)
                    .Invoke(context);
            });
        }
    }
}