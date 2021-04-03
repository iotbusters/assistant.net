using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Assistant.Net.Messaging.Internal
{
    public class RemoteCommandHandlingEndpointMiddleware
    {
        private readonly ISystemLifetime lifetime;
        private readonly IOptions<JsonSerializerOptions> serializerOptions;
        private readonly ICommandClient client;

        public RemoteCommandHandlingEndpointMiddleware(
            ISystemLifetime lifetime,
            IOptions<JsonSerializerOptions> serializerOptions,
            IOptionsMonitor<CommandOptions> commandOptions,
            IServiceScopeFactory scopeFactory)
        {
            this.serializerOptions = serializerOptions;
            this.lifetime = lifetime;

            var options = Options.Create(commandOptions.Get("Remote"));
            var provider = new ServiceCollection()
                .AddCommandClient()
                .Replace(ServiceDescriptor.Singleton(options))
                .Replace(ServiceDescriptor.Singleton(scopeFactory))
                .BuildServiceProvider();
            this.client = provider.GetRequiredService<ICommandClient>();
        }

        public virtual async Task Invoke(HttpContext httpContext)
        {
            var commandName = httpContext.GetRouteValue("commandName") as string
                              ?? throw new NotSupportedException();
            // todo
            var commandType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.IsClass && x.Name == commandName)
                ?? throw new NotSupportedException();

            dynamic command = await JsonSerializer.DeserializeAsync(
                httpContext.Request.Body,
                commandType,
                serializerOptions.Value,
                lifetime.Stopping)
                ?? new NotSupportedException();

            httpContext.Response.StatusCode = 200;
            var response = await client.Send(command);
            await JsonSerializer.SerializeAsync(
                httpContext.Response.Body,
                response,
                response.GetType(),
                serializerOptions.Value,
                lifetime.Stopping);
        }
    }
}