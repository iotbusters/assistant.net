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
        /// <summary>
        ///     Gets a header by <paramref name="name"/> and fails if not found.
        /// </summary>
        /// <exception cref="CommandContractException" />
        public static string GetRequiredHeader(this HttpContext httpContext, string name)
        {
            if (!httpContext.Request.Headers.TryGetValue(name, out var values))
                throw new CommandContractException($"Header '{name}' is required.");

            return values.First();
        }

        /// <summary>
        ///     Gets a command name from headers.
        /// </summary>
        /// <exception cref="CommandContractException" />
        public static string GetCommandName(this HttpContext httpContext) =>
            httpContext.GetRequiredHeader(HeaderNames.CommandName);

        /// <summary>
        ///     Gets a correlation ID from headers.
        /// </summary>
        /// <exception cref="CommandContractException" />
        public static string GetCorrelationId(this HttpContext httpContext) => httpContext
            .GetRequiredHeader(HeaderNames.CorrelationId);

        /// <summary>
        ///     Resolves a command type.
        /// </summary>
        /// <exception cref="CommandContractException" />
        /// <exception cref="CommandNotFoundException" />
        public static Type GetCommandType(this HttpContext httpContext)
        {
            var commandName = httpContext.GetCommandName();
            return httpContext.GetService<ITypeEncoder>().Decode(commandName)
                ?? throw new CommandNotFoundException($"Unknown command '{commandName}' was provided.");
        }

        /// <summary>
        ///     Reads command object from request body stream.
        /// </summary>
        /// <exception cref="CommandContractException" />
        public static async Task<object> ReadCommandObject(this HttpContext httpContext)
        {
            var commandType = httpContext.GetCommandType();
            var options = httpContext.GetService<IOptions<JsonSerializerOptions>>();
            var lifetime = httpContext.GetService<ISystemLifetime>();

            return await httpContext.Request.Body.ReadObject(commandType, options.Value, lifetime.Stopping)
                ?? throw new CommandContractException("Unexpected null object.");
        }

        /// <summary>
        ///     Writes the <paramref name="content"/> object to response and set <paramref name="statusCode"/>.
        /// </summary>
        public static async Task WriteCommandResponse(this HttpContext context, int statusCode, object? content = null)
        {
            var serializerOptions = context.GetService<IOptions<JsonSerializerOptions>>();
            var lifetime = context.GetService<ISystemLifetime>();

            context.Response.StatusCode = statusCode;

            if(content != null)
                await JsonSerializer.SerializeAsync(
                    context.Response.Body,
                    content,
                    content.GetType(),
                    serializerOptions.Value,
                    lifetime.Stopping);
        }

        /// <summary>
        ///     Writes the <paramref name="exception"/> failure object to response and set error <paramref name="statusCode"/>.
        /// </summary>
        public static async Task WriteCommandResponse(this HttpContext context, int statusCode, CommandException exception)
        {
            context.GetLogger().LogError(exception, "Remote command handling has failed.");

            await context.WriteCommandResponse(statusCode, (object)exception);
        }

        /// <summary>
        ///     Resolves default logger for the namespace.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static ILogger GetLogger(this HttpContext context) => context
            .GetService<ILoggerFactory>()
            .CreateLogger(typeof(HttpContextExtensions).Namespace);

        /// <summary>
        ///     Resolves a <typeparamref name="T"/> service configured in DI.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static T GetService<T>(this HttpContext context)
            where T : class => context
            .RequestServices.GetRequiredService<T>();
    }
}