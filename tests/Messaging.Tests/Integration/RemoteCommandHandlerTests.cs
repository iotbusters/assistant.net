using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Messaging.Tests.Mocks;
using Assistant.Net.Messaging.Tests.Mocks.Stubs;
using Assistant.Net.Messaging.Tests.Fixtures;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Tests.Integration
{
    public class RemoteCommandHandlerTests
    {
        [Test]
        public async Task Remote_returnsResponse()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemote<TestCommandHandler1>()
            .Create();

            var response = await fixture.Client.Send(new TestCommand1(0));

            response.Should().Be(new TestResponse(false));
        }

        [Test]
        public void Remote_throwsCommandNotRegisteredException_NoLocalHandler()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemote<TestCommandHandler2>()
            .Create();

            fixture.Client.Awaiting(x => x.Send(new TestCommand1(0)))
            .Should().Throw<CommandNotRegisteredException>()
            .WithMessage("Command TestCommand1 wasn't registered.")
            .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Remote_throwsCommandNotRegisteredException_NoRemoteHandler()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemoteCommandRegistrationOnly<TestCommand1>()
            .AddRemote<TestCommandHandler2>()// to have at least one handler configured
            .Create();

            fixture.Client.Awaiting(x => x.Send(new TestCommand1(0)))
            .Should().Throw<CommandNotRegisteredException>()
            .WithMessage("Command TestCommand1 wasn't registered.")
            .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Remote_throwsCommandFailedException_thrownInvalidOperationException()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemote<TestCommandHandler1>()
            .Create();

            fixture.Client.Awaiting(x => x.Send(new TestCommand1(1)))
            .Should().Throw<CommandFailedException>()
            .WithMessage("Command execution has failed.")
            .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Remote_throwsCommandFailedException_thrownCommandFailedException()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemote<TestCommandHandler1>()
            .Create();

            fixture.Client.Awaiting(x => x.Send(new TestCommand1(2)))
            .Should().Throw<CommandFailedException>()
            .WithMessage("2")
            .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Remote_throwsCommandFailedException_thrownCommandFailedExceptionWithInnerException()
        {
            using var fixture = new CommandClientFixtureBuilder()
            .AddRemote<TestCommandHandler1>()
            .Create();

            fixture.Client.Awaiting(x => x.Send(new TestCommand1(3)))
            .Should().Throw<CommandFailedException>()
            .WithMessage("3")
            .WithInnerException<CommandFailedException>()
            .Which.InnerException?.Message.Should().Be("inner");
        }
    }
}