using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Strongly typed http client for remote message handling.
    /// </summary>
    internal class WebMessageHandlerClient : IWebMessageHandlerClient
    {
        private readonly HttpClient client;
        private readonly ITypeEncoder typeEncoder;
        private readonly ISerializerFactory factory;

        public WebMessageHandlerClient(
            HttpClient client,
            ITypeEncoder typeEncoder,
            ISerializerFactory factory)
        {
            this.client = client;
            this.typeEncoder = typeEncoder;
            this.factory = factory;
        }

        /// <summary>
        ///     Delegates <paramref name="message"/> handling to remote WEB server.
        /// </summary>
        public async Task<TResponse> DelegateHandling<TResponse>(IMessage<TResponse> message, CancellationToken token)
        {
            var messageType = message.GetType();
            var requestSerializer = factory.Create(messageType);

            var requestStream = new MemoryStream();
            await requestSerializer.SerializeObject(requestStream, message, token);
            requestStream.Position = 0;

            var messageName = typeEncoder.Encode(messageType);

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "")
            {
                Headers = {{HeaderNames.MessageName, messageName}},
                Content = new StreamContent(requestStream)
            }, token);

            var responseSerializer = factory.Create(typeof(TResponse));
            var responseStream = await response.Content.ReadAsStreamAsync(token);
            var responseObject = (TResponse) await responseSerializer.DeserializeObject(responseStream, token);
            return responseObject!;
        }
    }
}
