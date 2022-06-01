using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Extensions;
using Assistant.Net.Messaging.Web.Client.Tests.Mocks;
using Assistant.Net.Serialization.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Client.Tests;

public class RemoteMessageHandlingClientTests
{
    private const string RequestUri = "http://localhost";
    private static readonly TestScenarioMessage ValidMessage = new(0);
    private static readonly TestResponse SuccessResponse = new(true);
    private static readonly MessageFailedException FailedResponse = new("test");

    private static Task<byte[]> Binary<T>(T value) => Provider.GetRequiredService<ISerializer<T>>().Serialize(value);
    private static readonly IServiceProvider Provider = new ServiceCollection().ConfigureJsonSerialization().BuildServiceProvider();

    [Test]
    public async Task DelegateHandling_sendsHttpRequestMessage()
    {
        var handler = new TestDelegatingHandler(await Binary(SuccessResponse), HttpStatusCode.OK);
        var client = Client(handler);

        await client.DelegateHandling(ValidMessage);

        handler.Request.Should().BeEquivalentTo(new HttpRequestMessage(HttpMethod.Post, RequestUri)
        {
            Headers = { { ClientHeaderNames.MessageName, nameof(TestScenarioMessage) } },
            Content = new ByteArrayContent(await Binary(SuccessResponse))
        });
    }

    [Test]
    public async Task DelegateHandling_returnsTestResponse()
    {
        var handler = new TestDelegatingHandler(await Binary(SuccessResponse), HttpStatusCode.OK);
        var client = Client(handler);

        var response = await client.DelegateHandling(ValidMessage);

        response.Should().Be(SuccessResponse);
    }

    [Test]
    public async Task DelegateHandling_throwMessageFailedException()
    {
        var handler = new TestDelegatingHandler(await Binary(FailedResponse), HttpStatusCode.InternalServerError);
        var client = Client(handler);

        await client.Awaiting(x => x.DelegateHandling(ValidMessage))
            .Should().ThrowAsync<MessageFailedException>()
            .WithMessage("test");
    }

    private static IWebMessageHandlerClient Client(DelegatingHandler handler)
    {
        var services = new ServiceCollection();
        services
            .AddRemoteWebMessagingClient()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(RequestUri))
            .ClearAllHttpMessageHandlers()
            .AddHttpMessageHandler<ErrorPropagationHandler>()
            .AddHttpMessageHandler(() => handler);
        return services
            .BuildServiceProvider()
            .GetRequiredService<IWebMessageHandlerClient>();
    }
}
