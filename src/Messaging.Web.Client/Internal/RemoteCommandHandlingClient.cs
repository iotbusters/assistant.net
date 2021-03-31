using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Assistant.Net.Core.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Messaging.Web.Client.Internal
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
            var httpResponse = await client.PostAsJsonAsync("", command, options.Value, lifetime.Stopping);
            var stream = await httpResponse.Content.ReadAsStreamAsync(lifetime.Stopping);
            var response = await JsonSerializer.DeserializeAsync<TResponse>(stream, options.Value, lifetime.Stopping);
            return response!;
        }
    }
}