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
        public async Task Send_returnsResponseObject()
        {
            var client = new ServiceCollection()
                .AddMessagingClient(b => b.AddLocalHandler<TestMessageHandler>().ClearInterceptors())
                .BuildServiceProvider()
                .GetRequiredService<IMessagingClient>();

            var response = await client.SendObject(new TestMessage(0));
            response.Should().BeOfType<TestResponse>();
        }

        [Test]
        public async Task Send_throwsException_noMessageHandlerRegistration()
        {
            var client = new ServiceCollection()
                .AddMessagingClient(b => b.ClearInterceptors())
                .BuildServiceProvider()
                .GetRequiredService<IMessagingClient>();

            await client.Awaiting(x => x.SendObject(new TestMessage(0)))
                .Should().ThrowAsync<MessageNotRegisteredException>();
        }

        [Test]
        public async Task Send_throwsException_handlingFailed()
        {
            var client = new ServiceCollection()
                .AddMessagingClient(b => b.AddLocalHandler<TestMessageHandler>().ClearInterceptors())
                .BuildServiceProvider()
                .GetRequiredService<IMessagingClient>();

            await client.Awaiting(x => x.SendObject(new TestMessage(1)))
                .Should().ThrowAsync<InvalidOperationException>().WithMessage("test");
        }

        [Test]
        public async Task Send_returnsResponseObject_defaultInterceptors()
        {
            var client = new ServiceCollection()
                .AddMessagingClient(b => b.AddLocalHandler<TestMessageHandler>())
                .BuildServiceProvider()
                .GetRequiredService<IMessagingClient>();

            var response = await client.SendObject(new TestMessage(0));
            response.Should().BeOfType<TestResponse>();
        }
    }
}
