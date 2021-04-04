using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Microsoft.Extensions.Options.Options;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Serialization;
using static Assistant.Net.Messaging.Options.Options;

namespace Assistant.Net.Messaging.Internal
{
    internal static class HttpContextExtensions
    {
        public static T GetService<T>(this HttpContext httpContext)
            where T : class => httpContext
            .RequestServices.GetRequiredService<T>();

        public static Type GetCommandType(this HttpContext httpContext)
        {
            var commandName = httpContext.Request.Headers["command-name"].FirstOrDefault()
                              ?? throw new CommandContractException("Header 'command-name' is required.");
            // todo: introduce type resolver
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

        public static ICommandClient GetClient(this HttpContext context)
        {
            var lifetime = context.GetService<ISystemLifetime>();
            var clock = context.GetService<ISystemClock>();
            var serializerOptions = context.GetService<IOptions<JsonSerializerOptions>>();
            var commandOptions = context.GetService<IOptionsMonitor<Options.CommandOptions>>();
            var options = Create(commandOptions.Get(RemoteName));
            var scopeFactory = context.GetService<IServiceScopeFactory>();

            var provider = new ServiceCollection()
                .AddCommandClient()
                .Replace(ServiceDescriptor.Singleton(lifetime))
                .Replace(ServiceDescriptor.Singleton(clock))
                .Replace(ServiceDescriptor.Singleton(serializerOptions))
                .Replace(ServiceDescriptor.Singleton(options))
                .Replace(ServiceDescriptor.Singleton(scopeFactory))
                .BuildServiceProvider();
            return provider.GetRequiredService<ICommandClient>();
        }
    }
}