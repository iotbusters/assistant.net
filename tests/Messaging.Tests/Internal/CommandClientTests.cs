using System;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Internal
{
    public class CommandClientTests
    {
        [Test]
        public async Task Send_returnsResponseObject()
        {
            var client = new ServiceCollection()
                .AddCommandClient(b => b.AddLocal<TestCommandHandler>().ClearInterceptors())
                .BuildServiceProvider()
                .GetRequiredService<ICommandClient>();

            var response = await client.Send(new TestCommand(0));
            response.Should().BeOfType<TestResponse>();
        }

        [Test]
        public async Task Send_throwsException_noCommandHandlerRegistration()
        {
            var client = new ServiceCollection()
                .AddCommandClient(b => b.ClearInterceptors())
                .BuildServiceProvider()
                .GetRequiredService<ICommandClient>();

            await client.Awaiting(x => x.Send(new TestCommand(0)))
                .Should().ThrowAsync<CommandNotRegisteredException>();
        }

        [Test]
        public async Task Send_throwsException_handlingFailed()
        {
            var client = new ServiceCollection()
                .AddCommandClient(b => b.AddLocal<TestCommandHandler>().ClearInterceptors())
                .BuildServiceProvider()
                .GetRequiredService<ICommandClient>();

            await client.Awaiting(x => x.Send(new TestCommand(1)))
                .Should().ThrowAsync<InvalidOperationException>().WithMessage("test");
        }
    }
}