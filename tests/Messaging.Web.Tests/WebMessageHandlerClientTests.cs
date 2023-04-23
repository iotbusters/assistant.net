using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Extensions;
using Assistant.Net.Messaging.Web.Tests.Mocks;
using Assistant.Net.Serialization.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Tests;

public class RemoteMessageHandlingClientTests
{
    private const string RequestUri = "http://localhost";
    private static readonly TestScenarioMessage validMessage = new(0);
    private static readonly TestResponse successResponse = new(true);
    private static readonly MessageFailedException failedResponse = new("test");
    private static readonly IServiceProvider provider = new ServiceCollection().ConfigureJsonSerialization().BuildServiceProvider();

    [Test]
    public async Task DelegateHandling_sendsHttpRequestMessage()
    {
        var handler = new TestDelegatingHandler(await Binary(successResponse), HttpStatusCode.OK);
        var client = Client(handler);

        await client.DelegateHandling(validMessage);

        handler.Request.Should().BeEquivalentTo(new HttpRequestMessage(HttpMethod.Post, RequestUri + "/messages")
        {
            Headers = {{ClientHeaderNames.MessageName, nameof(TestScenarioMessage)}},
            Content = new ByteArrayContent(await Binary(successResponse))
        });
    }

    [Test]
    public async Task DelegateHandling_returnsTestResponse()
    {
        var handler = new TestDelegatingHandler(await Binary(successResponse), HttpStatusCode.OK);
        var client = Client(handler);

        var response = await client.DelegateHandling(validMessage);

        response.Should().Be(successResponse);
    }

    [Test]
    public async Task DelegateHandling_throwMessageFailedException()
    {
        var handler = new TestDelegatingHandler(await Binary(failedResponse), HttpStatusCode.InternalServerError);
        var client = Client(handler);

        await client.Awaiting(x => x.DelegateHandling(validMessage))
            .Should().ThrowAsync<MessageFailedException>()
            .WithMessage("test");
    }

    private static Task<byte[]> Binary<T>(T value) => provider.GetRequiredService<ISerializer<T>>().Serialize(value);

    private static IWebMessageHandlerClient Client(DelegatingHandler handler)
    {
        var services = new ServiceCollection();
        services
            .ConfigureJsonSerialization()
            .AddWebMessageHandlerClient()
            .ConfigureHttpClient(c => c.BaseAddress = new(RequestUri))
            .ClearAllHttpMessageHandlers()
            .AddHttpMessageHandler<ErrorPropagationHandler>()
            .AddHttpMessageHandler(() => handler);
        return services
            .BuildServiceProvider()
            .GetRequiredService<IWebMessageHandlerClient>();
    }
}
