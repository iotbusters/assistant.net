using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Tests.TestObjects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Assistant.Net.Messaging.Tests
{
    public class CommandClientTests
    {
        private ICommandClient client = null!;

        [SetUp]
        public void Setup()
        {
            client = new ServiceCollection()
                .AddCommandClient(b => b.AddHandler<TestCommandHandler1>())
                .BuildServiceProvider()
                .GetRequiredService<ICommandClient>();
        }

        [Test]
        public async Task CommandReturnsResponse()
        {
            var command = new TestCommand1(exception: null);
            var response = await client.Send(command);

            response.Should().BeEquivalentTo(new TestResponse(false));
        }

        [Test]
        public async Task CommandThrowsCommandExecutionException()
        {
            var command = new TestCommand1(exception: new TestCommandExecutionException());
            await client.Awaiting(x => x.Send(command))
                .Should().ThrowAsync<TestCommandExecutionException>();
        }

        [Test]
        public async Task CommandThrowsException()
        {
            var command = new TestCommand1(exception: new Exception());
            await client.Awaiting(x => x.Send(command))
                .Should().ThrowAsync<CommandFailedException>();
        }

        [Test]
        public async Task CommandThrowsInvalidOperationException()
        {
            var command = new TestCommand1(exception: new InvalidOperationException());
            await client.Awaiting(x => x.Send(command))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Test]
        public async Task CommandThrowsOperationCanceledException()
        {
            var command = new TestCommand1(exception: new OperationCanceledException());
            await client.Awaiting(x => x.Send(command))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        [Test]
        public async Task CommandThrowsTimeoutException()
        {
            var command = new TestCommand1(exception: new TimeoutException());
            await client.Awaiting(x => x.Send(command))
                .Should().ThrowAsync<TimeoutException>();
        }
    }
}