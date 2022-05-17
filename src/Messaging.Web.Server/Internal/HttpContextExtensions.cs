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

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Common operations over <see cref="HttpContext" />.
/// </summary>
internal static class HttpContextExtensions
{
    /// <summary>
    ///     Gets a header by <paramref name="name"/> and fails if not found.
    /// </summary>
    /// <exception cref="MessageContractException" />
    public static string GetRequiredHeader(this HttpContext httpContext, string name)
    {
        if (!httpContext.Request.Headers.TryGetValue(name, out var values))
            throw new MessageContractException($"Header '{name}' is required.");

        return values.First();
    }

    /// <summary>
    ///     Gets a message name from headers.
    /// </summary>
    /// <exception cref="MessageContractException" />
    public static string GetMessageName(this HttpContext httpContext) =>
        httpContext.GetRequiredHeader(ServerHeaderNames.MessageName);

    /// <summary>
    ///     Gets a correlation ID from headers.
    /// </summary>
    /// <exception cref="MessageContractException" />
    public static string GetCorrelationId(this HttpContext httpContext) => httpContext
        .GetRequiredHeader(ServerHeaderNames.CorrelationId);

    /// <summary>
    ///     Resolves a message type.
    /// </summary>
    /// <exception cref="MessageContractException" />
    /// <exception cref="MessageNotFoundException" />
    public static Type GetMessageType(this HttpContext httpContext)
    {
        var messageName = httpContext.GetMessageName();
        return httpContext.GetService<ITypeEncoder>().Decode(messageName)
               ?? throw new MessageNotFoundException("Couldn't resolve message type from its name.", messageName);
    }

    /// <summary>
    ///     Reads message object from request body stream.
    /// </summary>
    /// <exception cref="MessageContractException" />
    public static async Task<object> ReadMessageObject(this HttpContext httpContext)
    {
        var messageType = httpContext.GetMessageType();
        var factory = httpContext.GetService<ISerializerFactory>();
        var serializer = factory.Create(messageType);

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
            throw new MessageContractException($"Reading '{messageType.Name}' object has failed.", e);
        }
    }

    /// <summary>
    ///     Writes the <paramref name="content"/> object to response and set <paramref name="statusCode"/>.
    /// </summary>
    public static Task WriteMessageResponse(this HttpContext httpContext, int statusCode, object? content = null)
    {
        var messageName = httpContext.GetMessageName();
        var correlationId = httpContext.GetCorrelationId();

        httpContext.Response.Headers.TryAdd(ServerHeaderNames.MessageName, messageName);
        httpContext.Response.Headers.TryAdd(ServerHeaderNames.CorrelationId, correlationId);
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
    public static async Task WriteMessageResponse(this HttpContext context, int statusCode, MessageException exception)
    {
        context.GetLogger().LogError(exception, "Remote message handling has failed.");

        await context.WriteMessageResponse(statusCode, (object)exception);
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
