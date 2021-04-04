using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Messaging.Tests.Mocks;
using Assistant.Net.Messaging.Tests.Mocks.Stubs;
using Assistant.Net.Messaging.Tests.Fixtures;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Tests.Integration
{
    public class CommandHandlerTests
    {
        [Test]
        public async Task Remote_returnsResponse()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemote<TestCommandHandler1>()
            .Create();

            var response = await fixture.Client.Send(new TestCommand1(null));

            response.Should().Be(new TestResponse(false));
        }

        [Test]
        public void Remote_throwsCommandNotRegisteredException_NoLocalHandler()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemote<TestCommandHandler2>()
            .Create();

            fixture.Client.Awaiting(x => x.Send(new TestCommand1(null)))
            .Should().Throw<CommandNotRegisteredException>()
            .WithMessage("Command TestCommand1 wasn't registered.");
        }

        [Test]
        public void Remote_throwsCommandNotRegisteredException_NoRemoteHandler()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemoteCommandRegistrationOnly<TestCommand1>()
            .Create();

            fixture.Client.Awaiting(x => x.Send(new TestCommand1(null)))
            .Should().Throw<CommandNotRegisteredException>()
            .WithMessage("Command TestCommand1 wasn't registered.");
        }

        [Test]
        public void Remote_throwsCommandFailedException_thrownInvalidOperationException()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemote<TestCommandHandler1>()
            .Create();
            var propagatedException = new InvalidOperationException(
                "1",
                new ArgumentException("2"));

            fixture.Client.Awaiting(x => x.Send(new TestCommand1(propagatedException)))
            .Should().Throw<CommandFailedException>()
            .WithMessage("Command execution has failed.")
            .WithInnerException<InvalidOperationException>();
        }
    }
}