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
    internal class RemoteWebCommandClient : IRemoteCommandClient
    {
        private readonly HttpClient client;
        private readonly ISystemLifetime lifetime;
        private readonly ITypeEncoder typeEncoder;
        private readonly IOptions<JsonSerializerOptions> options;

        public RemoteWebCommandClient(
            HttpClient client,
            ISystemLifetime lifetime,
            ITypeEncoder typeEncoder,
            IOptions<JsonSerializerOptions> options)
        {
            this.client = client;
            this.lifetime = lifetime;
            this.typeEncoder = typeEncoder;
            this.options = options;
        }

        /// <summary>
        ///     Delegates <paramref name="command"/> handling to remote WEB server.
        /// </summary>
        public async Task<TResponse> DelegateHandling<TResponse>(ICommand<TResponse> command)
        {
            var commandName = typeEncoder.Encode(command.GetType());
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "")
            {
                Headers =
                { 
                    { HeaderNames.CommandName, commandName }
                },
                Content = JsonContent.Create(command, command.GetType(), options: options.Value)
            }, lifetime.Stopping);

            var stream = await response.Content.ReadAsStreamAsync(lifetime.Stopping);
            var responseObject = await JsonSerializer.DeserializeAsync<TResponse>(stream, options.Value, lifetime.Stopping);
            return responseObject!;
        }
    }
}