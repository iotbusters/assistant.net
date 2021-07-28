using System.IO;
using System.Net.Http;
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

            var requestStream = new MemoryStream();
            await requestSerializer.SerializeObject(requestStream, command);
            requestStream.Position = 0;

            var commandName = typeEncoder.Encode(commandType);

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "")
            {
                Headers = {{HeaderNames.CommandName, commandName}},
                Content = new StreamContent(requestStream)
            }, lifetime.Stopping);

            var responseSerializer = factory.Create(typeof(TResponse));
            var responseStream = await response.Content.ReadAsStreamAsync(lifetime.Stopping);
            var responseObject = (TResponse) await responseSerializer.DeserializeObject(responseStream);
            return responseObject!;
        }
    }
}