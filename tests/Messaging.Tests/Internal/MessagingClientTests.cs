using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Internal;

public class MessagingClientTests
{
    [Test]
    public async Task RequestObject_returnsResponseObject()
    {
        var client = new ServiceCollection()
            .AddMessagingClient(b => b.AddHandler<TestScenarioMessageHandler>().ClearInterceptors())
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        var response = await client.RequestObject(new TestMessage(0));

        response.Should().BeOfType<TestResponse>();
    }

    [Test]
    public async Task RequestObject_throwsException_noMessageHandlerRegistration()
    {
        var client = new ServiceCollection()
            .AddMessagingClient(b => b.ClearInterceptors())
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.Awaiting(x => x.RequestObject(new TestMessage(0)))
            .Should().ThrowAsync<MessageNotRegisteredException>();
    }

    [Test]
    public async Task RequestObject_throwsException_handlingFailed()
    {
        var client = new ServiceCollection()
            .AddMessagingClient(b => b.AddHandler<TestScenarioMessageHandler>().ClearInterceptors())
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.Awaiting(x => x.RequestObject(new TestMessage(1)))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("test");
    }

    [Test]
    public async Task RequestObject_callsInterceptor_specificMessage()
    {
        var interceptor = new TestMessageRequestInterceptor<TestMessage, TestResponse>();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage, TestResponse>(new()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.RequestObject(new TestMessage(0));

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task RequestObject_callsInterceptor_referenceResponseTypeInterceptor()
    {
        var interceptor = new TestMessageRequestInterceptor<IMessage<object>, object>();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage, TestResponse>(new()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.RequestObject(new TestMessage(0));

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task RequestObject_callsInterceptor_valueResponseTypeInterceptor()
    {
        var interceptor = new TestMessageRequestInterceptor<IMessage<Guid>, Guid>();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage3, Guid>(Guid.NewGuid()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.RequestObject(new TestMessage3());

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task RequestObject_doesNotCallInterceptor_referenceResponseTypeInterceptorButValueResponseType()
    {
        var interceptor = new TestMessageRequestInterceptor<IMessage<object>, object>();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage3, Guid>(Guid.NewGuid()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.RequestObject(new TestMessage3());

        interceptor.CallCount.Should().Be(0);
    }

    [Test]
    public async Task RequestObject_callsInterceptor_abstractInterceptorButReferenceResponseType()
    {
        var interceptor = new TestAbstractInterceptor();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage, TestResponse>(new()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.RequestObject(new TestMessage(0));

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task RequestObject_callsInterceptor_abstractInterceptorButValueResponseType()
    {
        var interceptor = new TestAbstractInterceptor();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage3, Guid>(Guid.NewGuid()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.RequestObject(new TestMessage3());

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task RequestObject_returnsResponseObject_defaultInterceptors()
    {
        var client = new ServiceCollection()
            .AddMessagingClient(b => b.AddHandler<TestScenarioMessageHandler>().ClearInterceptors())
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        var response = await client.RequestObject(new TestMessage(0));

        response.Should().BeOfType<TestResponse>();
    }

    [Test]
    public async Task RequestObject_returnsResponse_namedClient()
    {
        var handler1 = new TestMessageHandler<TestMessage2, TestResponse2>(new(1));
        var handler2 = new TestMessageHandler<TestMessage2, TestResponse2>(new(2));
        var handler3 = new TestMessageHandler<TestMessage2, TestResponse2>(new(3));
        var mainProvider = new ServiceCollection()
            .AddMessagingClient(b => b.AddHandler(handler1).ClearInterceptors())
            .ConfigureMessagingClient("1", b => b.AddHandler(handler2).ClearInterceptors())
            .ConfigureMessagingClient("2", b => b.AddHandler(handler3).ClearInterceptors())
            .BuildServiceProvider();

        var provider1 = mainProvider.CreateScope().ServiceProvider;
        var response1 = await provider1.GetRequiredService<IMessagingClient>().RequestObject(new TestMessage2(0));
        response1.Should().Be(new TestResponse2(1));

        var provider2 = mainProvider.CreateScopeWithNamedOptionContext("1").ServiceProvider;
        var response2 = await provider2.GetRequiredService<IMessagingClient>().RequestObject(new TestMessage2(0));
        response2.Should().Be(new TestResponse2(2));

        var provider3 = mainProvider.CreateScopeWithNamedOptionContext("2").ServiceProvider;
        var response3 = await provider3.GetRequiredService<IMessagingClient>().RequestObject(new TestMessage2(0));
        response3.Should().Be(new TestResponse2(3));
    }

    [Test]
    public async Task PublishObject_delegatesToHandler()
    {
        var handler = new TestMessageHandler<TestMessage, TestResponse>(new(false));
        var client = new ServiceCollection()
            .AddMessagingClient(b => b.AddHandler(handler).ClearInterceptors())
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.PublishObject(new TestMessage(0));

        handler.Message.Should().BeOfType<TestMessage>();
    }

    [Test]
    public async Task PublishObject_throwsException_noMessageHandlerRegistration()
    {
        var client = new ServiceCollection()
            .AddMessagingClient(b => b.ClearInterceptors())
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.Awaiting(x => x.PublishObject(new TestMessage(0)))
            .Should().ThrowAsync<MessageNotRegisteredException>();
    }

    [Test]
    public async Task PublishObject_throwsException_handlingFailed()
    {
        var client = new ServiceCollection()
            .AddMessagingClient(b => b.AddHandler<TestScenarioMessageHandler>().ClearInterceptors())
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.Awaiting(x => x.PublishObject(new TestMessage(1)))
            .Should().ThrowAsync<InvalidOperationException>().WithMessage("test");
    }

    [Test]
    public async Task PublishObject_callsInterceptor_specificMessage()
    {
        var interceptor = new TestMessagePublishInterceptor<TestMessage>();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage, TestResponse>(new()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.PublishObject(new TestMessage(0));

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task PublishObject_callsInterceptor_referenceResponseTypeInterceptor()
    {
        var interceptor = new TestMessagePublishInterceptor<IMessage<object>>();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage, TestResponse>(new()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.PublishObject(new TestMessage(0));

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task PublishObject_callsInterceptor_valueResponseTypeInterceptor()
    {
        var interceptor = new TestMessagePublishInterceptor<IMessage<Guid>>();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage3, Guid>(Guid.NewGuid()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.PublishObject(new TestMessage3());

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task PublishObject_callsInterceptor_abstractInterceptorButReferenceResponseType()
    {
        var interceptor = new TestAbstractInterceptor();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage, TestResponse>(new()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.PublishObject(new TestMessage(0));

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task PublishObject_callsInterceptor_abstractInterceptorButValueResponseType()
    {
        var interceptor = new TestAbstractInterceptor();
        var client = new ServiceCollection()
            .AddMessagingClient(b => b
                .AddHandler(new TestMessageHandler<TestMessage, TestResponse>(new()))
                .ClearInterceptors()
                .AddInterceptor(interceptor))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.PublishObject(new TestMessage(0));

        interceptor.CallCount.Should().Be(1);
    }

    [Test]
    public async Task PublishObject_returnsResponseObject_defaultInterceptors()
    {
        var handler = new TestMessageHandler<TestMessage, TestResponse>(new(false));
        var client = new ServiceCollection()
            .AddMessagingClient(b => b.UseLocal().AddHandler(handler))
            .BuildServiceProvider()
            .GetRequiredService<IMessagingClient>();

        await client.PublishObject(new TestMessage(0));

        handler.Message.Should().BeEquivalentTo(new TestMessage(0));
    }

    [Test]
    public async Task PublishObject_returnsResponse_namedClient()
    {
        var handler1 = new TestMessageHandler<TestMessage2, TestResponse2>(new(1));
        var handler2 = new TestMessageHandler<TestMessage2, TestResponse2>(new(2));
        var handler3 = new TestMessageHandler<TestMessage2, TestResponse2>(new(3));
        var mainProvider = new ServiceCollection()
            .AddMessagingClient(b => b.AddHandler(handler1).ClearInterceptors())
            .ConfigureMessagingClient("1", b => b.AddHandler(handler2).ClearInterceptors())
            .ConfigureMessagingClient("2", b => b.AddHandler(handler3).ClearInterceptors())
            .BuildServiceProvider();

        var provider1 = mainProvider.CreateScope().ServiceProvider;
        await provider1.GetRequiredService<IMessagingClient>().PublishObject(new TestMessage2(1));
        handler1.Message.Should().Be(new TestMessage2(1));

        var provider2 = mainProvider.CreateScopeWithNamedOptionContext("1").ServiceProvider;
        await provider2.GetRequiredService<IMessagingClient>().PublishObject(new TestMessage2(2));
        handler2.Message.Should().Be(new TestMessage2(2));

        var provider3 = mainProvider.CreateScopeWithNamedOptionContext("2").ServiceProvider;
        await provider3.GetRequiredService<IMessagingClient>().PublishObject(new TestMessage2(3));
        handler3.Message.Should().Be(new TestMessage2(3));
    }
}
