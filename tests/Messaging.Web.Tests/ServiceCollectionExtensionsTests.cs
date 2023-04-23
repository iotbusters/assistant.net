using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using FluentAssertions;
using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Web.Tests.Mocks;

namespace Assistant.Net.Messaging.Web.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
    public void GetServiceOfMessageExceptionJsonConverter_resolvesObject()
    {
        var provider = new ServiceCollection()
            .ConfigureJsonSerialization()
            .BuildServiceProvider();

        provider.GetService<MessageExceptionJsonConverter>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfSerializer_resolvesObject()
    {
        var provider = new ServiceCollection()
            .ConfigureJsonSerialization()
            .BuildServiceProvider();

        provider.GetService<ISerializer<object>>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfIMessagingClient_resolvesObject()
    {
        var provider = new ServiceCollection()
            .AddWebMessageHandling(delegate { })
            .ConfigureMessagingClient(b => b
                .AddHandler<TestFailMessageHandler>()) // to have at least one handler configured
            .BuildServiceProvider();

        provider.GetService<IMessagingClient>()
            .Should().NotBeNull();
    }

    [Test]
    public void GetServiceOfWebMessageHandlerClient_resolvesObject()
    {
        var provider = new ServiceCollection()
            .ConfigureJsonSerialization()
            .AddWebMessageHandlerClient()
            .ConfigureHttpClient(c => c.BaseAddress = new("http://localhost"))
            .Services.BuildServiceProvider();

        provider.GetService<IWebMessageHandlerClient>()
            .Should().NotBeNull();
    }
}
