using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Internal
{
    public class MessagingClientTests
    {
        [Test]
        public async Task RequestObject_returnsResponseObject()
        {
            var client = new ServiceCollection()
                .AddMessagingClient(b => b.AddLocalHandler<TestScenarioMessageHandler>().ClearInterceptors())
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
                .AddMessagingClient(b => b.AddLocalHandler<TestScenarioMessageHandler>().ClearInterceptors())
                .BuildServiceProvider()
                .GetRequiredService<IMessagingClient>();

            await client.Awaiting(x => x.RequestObject(new TestMessage(1)))
                .Should().ThrowAsync<InvalidOperationException>().WithMessage("test");
        }

        [Test]
        public async Task RequestObject_returnsResponseObject_defaultInterceptors()
        {
            var client = new ServiceCollection()
                .AddMessagingClient(b => b.AddLocalHandler<TestScenarioMessageHandler>())
                .BuildServiceProvider()
                .GetRequiredService<IMessagingClient>();

            var response = await client.RequestObject(new TestMessage(0));

            response.Should().BeOfType<TestResponse>();
        }

        [Test]
        public async Task PublishObject_returnsResponseObject()
        {
            var handler = new TestMessageHandler<TestMessage, TestResponse>(new TestResponse(false));
            var client = new ServiceCollection()
                .AddMessagingClient(b => b.AddLocalHandler(handler).ClearInterceptors())
                .BuildServiceProvider()
                .GetRequiredService<IMessagingClient>();

            await client.PublishObject(new TestMessage(0));

            handler.Message.Should().BeEquivalentTo(new TestMessage(0));
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
                .AddMessagingClient(b => b.AddLocalHandler<TestScenarioMessageHandler>().ClearInterceptors())
                .BuildServiceProvider()
                .GetRequiredService<IMessagingClient>();

            await client.Awaiting(x => x.PublishObject(new TestMessage(1)))
                .Should().ThrowAsync<InvalidOperationException>().WithMessage("test");
        }

        [Test]
        public async Task PublishObject_returnsResponseObject_defaultInterceptors()
        {
            var handler = new TestMessageHandler<TestMessage, TestResponse>(new TestResponse(false));
            var client = new ServiceCollection()
                .AddMessagingClient(b => b.AddLocalHandler(handler))
                .BuildServiceProvider()
                .GetRequiredService<IMessagingClient>();

            await client.PublishObject(new TestMessage(0));

            handler.Message.Should().BeEquivalentTo(new TestMessage(0));
        }
    }
}
