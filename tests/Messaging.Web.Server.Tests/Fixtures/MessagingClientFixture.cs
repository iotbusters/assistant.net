using Assistant.Net.Messaging.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Server.Tests.Fixtures;

public sealed class MessagingClientFixture : IDisposable
{
    public static readonly string CorrelationId = Guid.NewGuid().ToString();

    private readonly ServiceProvider provider;
    private readonly IHost host;

    public MessagingClientFixture(IHost host)
    {
        this.provider = new ServiceCollection()
            .AddSingleton(new HttpClient(host.GetTestServer().CreateHandler()))
            .ConfigureJsonSerialization()
            .BuildServiceProvider();
        this.host = host;
    }

    public async Task<HttpResponseMessage> Request(IAbstractMessage message)
    {
        var binary = Binary(message);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/messages")
        {
            Headers = {{ServerHeaderNames.MessageName, message.GetType().Name}, {ServerHeaderNames.CorrelationId, CorrelationId}},
            Content = new ByteArrayContent(binary)
        };

        var response = await Client.SendAsync(request);
        response.Should().BeEquivalentTo(new {RequestMessage = request});
        response.Headers.AsEnumerable().Should().BeEquivalentTo(new Dictionary<string, IEnumerable<string>>
        {
            {ServerHeaderNames.MessageName, new[] {message.GetType().Name}}, {ServerHeaderNames.CorrelationId, new[] {CorrelationId}}
        });

        return response;
    }

    public async Task<TResponse?> RequestObject<TResponse>(IAbstractMessage message)
    {
        var binary = Binary(message);
        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/messages")
        {
            Headers = {{ServerHeaderNames.MessageName, message.GetType().Name}, {ServerHeaderNames.CorrelationId, CorrelationId}},
            Content = new ByteArrayContent(binary)
        };

        var response = await Client.SendAsync(request);
        response.Should().BeEquivalentTo(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            RequestMessage = request,
            Headers = {{ServerHeaderNames.MessageName, message.GetType().Name}, {ServerHeaderNames.CorrelationId, CorrelationId}}
        });

        return await ReadObject<TResponse>(response);
    }

    public async Task<TResponse?> ReadObject<TResponse>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<TResponse>(Options);

    public void Dispose()
    {
        provider.Dispose();
        host.Dispose();
    }

    private HttpClient Client => provider.GetRequiredService<HttpClient>();
    private JsonSerializerOptions Options => provider.GetRequiredService<IOptions<JsonSerializerOptions>>().Value;
    private byte[] Binary(IAbstractMessage message) => JsonSerializer.SerializeToUtf8Bytes(message, Options);
}
