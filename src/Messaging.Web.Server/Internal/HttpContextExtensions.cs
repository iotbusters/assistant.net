using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                ?? throw new CommandNotFoundException("Couldn't resolve command type from its name.", commandName);
        }

        /// <summary>
        ///     Reads command object from request body stream.
        /// </summary>
        /// <exception cref="CommandContractException" />
        public static async Task<object> ReadCommandObject(this HttpContext httpContext)
        {
            var commandType = httpContext.GetCommandType();
            var factory = httpContext.GetService<ISerializerFactory>();
            var serializer = factory.Create(commandType);

            try
            {
                return await serializer.DeserializeObject(httpContext.Request.Body);
            }
            catch (SerializerTypeNotRegisteredException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CommandContractException($"Reading '{commandType.Name}' object has failed.", e);
            }
        }

        /// <summary>
        ///     Writes the <paramref name="content"/> object to response and set <paramref name="statusCode"/>.
        /// </summary>
        public static Task WriteCommandResponse(this HttpContext httpContext, int statusCode, object? content = null)
        {
            var commandName = httpContext.GetCommandName();
            var correlationId = httpContext.GetCorrelationId();

            httpContext.Response.Headers.TryAdd(HeaderNames.CommandName, commandName);
            httpContext.Response.Headers.TryAdd(HeaderNames.CorrelationId, correlationId);
            httpContext.Response.StatusCode = statusCode;

            if(content == null)
                return Task.CompletedTask;

            var factory = httpContext.GetService<ISerializerFactory>();
            var serializer = factory.Create(content.GetType());
            return serializer.SerializeObject(httpContext.Response.Body, content);
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
            where T : class => (T) context.GetService(typeof(T));

        /// <summary>
        ///     Resolves a <paramref name="serviceType"/> configured in DI.
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        public static object GetService(this HttpContext context, Type serviceType) => context
            .RequestServices.GetRequiredService(serviceType);
    }
}