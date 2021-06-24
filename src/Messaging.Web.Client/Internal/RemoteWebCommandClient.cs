using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Serialization.Abstractions;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Strongly typed http client for remote command handling.
    /// </summary>
    internal class RemoteWebCommandClient : IRemoteCommandClient
    {
        private readonly HttpClient client;
        private readonly ISystemLifetime lifetime;
        private readonly ITypeEncoder typeEncoder;
        private readonly ISerializerFactory factory;

        public RemoteWebCommandClient(
            HttpClient client,
            ISystemLifetime lifetime,
            ITypeEncoder typeEncoder,
            ISerializerFactory factory)
        {
            this.client = client;
            this.lifetime = lifetime;
            this.typeEncoder = typeEncoder;
            this.factory = factory;
        }

        /// <summary>
        ///     Delegates <paramref name="command"/> handling to remote WEB server.
        /// </summary>
        public async Task<TResponse> DelegateHandling<TResponse>(ICommand<TResponse> command)
        {
            var commandType = command.GetType();
            var requestSerializer = factory.Create(commandType);
            var responseSerializer = factory.Create<TResponse>();

            var commandName = typeEncoder.Encode(commandType);
            var requestBytes = requestSerializer.Serialize(command);

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "")
            {
                Headers =
                { 
                    { HeaderNames.CommandName, commandName }
                },
                Content = new ByteArrayContent(requestBytes)
            }, lifetime.Stopping);

            var stream = await response.Content.ReadAsStreamAsync(lifetime.Stopping);

            await using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);

            var responseObject = responseSerializer.Deserialize(memory.ToArray());
            return responseObject!;
        }
    }
}