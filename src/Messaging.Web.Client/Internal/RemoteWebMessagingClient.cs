using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Strongly typed http client for remote message handling.
    /// </summary>
    internal class RemoteWebMessagingClient : IRemoteMessagingClient
    {
        private readonly HttpClient client;
        private readonly ISystemLifetime lifetime;
        private readonly ITypeEncoder typeEncoder;
        private readonly ISerializerFactory factory;

        public RemoteWebMessagingClient(
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
        ///     Delegates <paramref name="message"/> handling to remote WEB server.
        /// </summary>
        public async Task<TResponse> DelegateHandling<TResponse>(IMessage<TResponse> message)
        {
            var messageType = message.GetType();
            var requestSerializer = factory.Create(messageType);

            var requestStream = new MemoryStream();
            await requestSerializer.SerializeObject(requestStream, message);
            requestStream.Position = 0;

            var messageName = typeEncoder.Encode(messageType);

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "")
            {
                Headers = {{HeaderNames.MessageName, messageName}},
                Content = new StreamContent(requestStream)
            }, lifetime.Stopping);

            var responseSerializer = factory.Create(typeof(TResponse));
            var responseStream = await response.Content.ReadAsStreamAsync(lifetime.Stopping);
            var responseObject = (TResponse) await responseSerializer.DeserializeObject(responseStream);
            return responseObject!;
        }
    }
}