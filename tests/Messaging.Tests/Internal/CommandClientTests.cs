using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Tests.Mocks;
using Assistant.Net.Messaging.Tests.Mocks.Stubs;

namespace Assistant.Net.Messaging.Tests.Internal
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
            var command = new TestCommand1(Exception: null);
            var response = await client.Send(command);

            response.Should().BeEquivalentTo(new TestResponse(Fail: false));
        }

        [Test]
        public async Task ThrowsException()
        {
            var command = new TestCommand1(Exception: new Exception());
            await client.Awaiting(x => x.Send(command))
                .Should().ThrowAsync<Exception>();
        }
    }
}