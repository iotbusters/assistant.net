using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Serialization.Abstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

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
    public async Task<object> DelegateHandling(object message, CancellationToken token)
    {
        var messageType = message.GetType();
        if (!messageType.IsMessage())
            throw new ArgumentException($"Expected message but provided {messageType}.", nameof(messageType));

        var responseType = messageType.GetResponseType()!;
        var requestSerializer = factory.Create(messageType);

        var requestStream = new MemoryStream();
        await requestSerializer.SerializeObject(requestStream, message, token);
        requestStream.Position = 0;

        var messageName = typeEncoder.Encode(messageType);

        using var request = new HttpRequestMessage(HttpMethod.Post, "messages")
        {
            Headers = {{ClientHeaderNames.MessageName, messageName}},
            Content = new StreamContent(requestStream)
        };
        var response = await client.SendAsync(request, token);

        var responseSerializer = factory.Create(responseType);
        await using var responseStream = await response.Content.ReadAsStreamAsync(token);
        var responseObject = await responseSerializer.DeserializeObject(responseStream, token);
        return responseObject!;
    }
}
