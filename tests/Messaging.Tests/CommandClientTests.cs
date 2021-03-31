using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Configuration;
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
                .AddCommandClient(b =>
                {
                    b.Handlers.AddLocal<TestCommandHandler1>();
                    b.Interceptors.Clear();
                })
                .BuildServiceProvider()
                .GetRequiredService<ICommandClient>();
        }

        [Test]
        public async Task ReturnsResponse()
        {
            var command = new TestCommand1(exception: null);
            var response = await client.Send(command);

            response.Should().BeEquivalentTo(new TestResponse(false));
        }

        [Test]
        public async Task ThrowsException()
        {
            var command = new TestCommand1(exception: new Exception());
            await client.Awaiting(x => x.Send(command))
                .Should().ThrowAsync<Exception>();
        }
    }
}