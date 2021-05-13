using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Strongly typed http client for remote command handling.
    /// </summary>
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
                // todo: add request-id (https://github.com/iotbusters/assistant.net/issues/6)
                Headers = { { HeaderNames.CommandName, commandName } },
                Content = JsonContent.Create(command, command.GetType(), options: options.Value)
            }, lifetime.Stopping);

            var stream = await response.Content.ReadAsStreamAsync(lifetime.Stopping);
            var responseObject = await JsonSerializer.DeserializeAsync<TResponse>(stream, options.Value, lifetime.Stopping);
            return responseObject!;
        }
    }
}