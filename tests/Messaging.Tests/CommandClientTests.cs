using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Core;
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
        private ICommandClient client;

        [SetUp]
        public void Setup()
        {
            client = new ServiceCollection()
                .AddSystemLifetime(p => CancellationToken.None)
                .AddCommandOptions(b => b.AddHandler<TestCommandHandler1>())
                .AddSingleton<ICommandClient, CommandClient>()
                .BuildServiceProvider()
                .GetRequiredService<ICommandClient>();
        }

        [Test]
        public async Task Test1()
        {
            var response = await client.Send(new TestCommand1(fail: false));

            response.Should().BeEquivalentTo(new TestResponse(false));
        }

        [Test]
        public async Task Test2()
        {
            await client.Awaiting(x => x.Send(new TestCommand1(fail: true)))
                .Should().ThrowAsync<CommandFailedException>()
                .WithMessage(nameof(TestCommandHandler1));
        }
    }
}