using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Serialization;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Common operations over <see cref="HttpContext" />.
    /// </summary>
    internal static class HttpContextExtensions
    {
        public static string GetRequiredHeader(this HttpContext httpContext, string name)
        {
            if (!httpContext.Request.Headers.TryGetValue(name, out var values))
                throw new CommandContractException($"Header '{name}' is required.");

            return values.First();
        }

        public static string GetCommandName(this HttpContext httpContext) =>
            httpContext.GetRequiredHeader(HeaderNames.CommandName);

        public static Guid GetCorrelationId(this HttpContext httpContext)
        {
            var value = httpContext.GetRequiredHeader(HeaderNames.CorrelationIdName);

            if (!Guid.TryParse(value, out var correlationId))
                throw new CommandContractException($"Header '{HeaderNames.CorrelationIdName}' is invalid.");

            return correlationId;
        }

        public static Type GetCommandType(this HttpContext httpContext)
        {
            var commandName = httpContext.GetCommandName();
            // todo: introduce type resolver (https://github.com/iotbusters/assistant.net/issues/7)
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.IsClass && x.Name == commandName)
                ?? throw new CommandNotFoundException($"Unknown command '{commandName}' was provided.");
        }

        public static async Task<object> ReadCommandObject(this HttpContext context)
        {
            var commandType = context.GetCommandType();
            var options = context.GetService<IOptions<JsonSerializerOptions>>();
            var lifetime = context.GetService<ISystemLifetime>();

            return await context.Request.Body.ReadObject(
                commandType,
                options.Value,
                lifetime.Stopping)
                ?? throw new CommandContractException("Unexpected null object.");
        }

        public static async Task WriteCommandResponse(this HttpContext context, int statusCode, object content)
        {
            var serializerOptions = context.GetService<IOptions<JsonSerializerOptions>>();
            var lifetime = context.GetService<ISystemLifetime>();

            context.Response.StatusCode = statusCode;

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                content,
                content.GetType(),
                serializerOptions.Value,
                lifetime.Stopping);
        }

        public static async Task WriteCommandResponse(this HttpContext context, int statusCode, CommandException exception)
        {
            context
                .GetService<ILoggerFactory>()
                .CreateLogger(typeof(HttpContextExtensions).Namespace)
                .LogError(exception, "Remote command handling has failed.");

            await context.WriteCommandResponse(statusCode, (object)exception);
        }

        public static T GetService<T>(this HttpContext httpContext)
            where T : class => httpContext
            .RequestServices.GetRequiredService<T>();
    }
}