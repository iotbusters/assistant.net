using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Messaging.Internal
{
    public class RemoteCommandHandlingClient
    {
        private readonly HttpClient client;
        private readonly ISystemLifetime lifetime;
        private readonly IOptions<JsonSerializerOptions> options;

        public RemoteCommandHandlingClient(
            HttpClient client,
            ISystemLifetime lifetime,
            IOptions<JsonSerializerOptions> options)
        {
            this.client = client;
            this.lifetime = lifetime;
            this.options = options;
        }

        public async Task<TResponse> DelegateHandling<TResponse>(ICommand<TResponse> command)
        {
            var commandName = command.GetType().Name;
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "")
            {
                // todo: add tracing ids, ETag etc
                Headers = { { "command-name", commandName } },
                Content = JsonContent.Create(command, command.GetType(), options: options.Value)
            }, lifetime.Stopping);

            var str = await response.Content.ReadAsStringAsync(lifetime.Stopping);
            var stream = await response.Content.ReadAsStreamAsync(lifetime.Stopping);
            var responseObject = await JsonSerializer.DeserializeAsync<TResponse>(stream, options.Value, lifetime.Stopping);
            return responseObject!;
        }
    }
}