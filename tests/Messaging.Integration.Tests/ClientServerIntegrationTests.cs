using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Integration.Tests.Fixtures;
using Assistant.Net.Messaging.Integration.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;

namespace Assistant.Net.Messaging.Integration.Tests
{
    [Timeout(4000)]
    public class ClientServerIntegrationTests
    {
        [Test]
        public async Task Send_returnsResponse()
        {
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestScenarioCommandHandler>()
                .Create();

            var response = await fixture.Client.Send(new TestScenarioCommand(0));

            response.Should().Be(new TestResponse(false));
        }

        [Test]
        public void Send_throwsCommandNotRegisteredException_NoLocalHandler()
        {
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestSuccessFailureCommandHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.Send(new TestScenarioCommand(0)))
                .Should().ThrowExactly<CommandNotRegisteredException>()
                .WithMessage($"Command '{nameof(TestScenarioCommand)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsCommandNotRegisteredException_NoRemoteHandler()
        {
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemoteCommandRegistrationOnly<TestScenarioCommand>()
                .AddRemote<TestSuccessFailureCommandHandler>()// to have at least one handler configured
                .Create();

            fixture.Client.Awaiting(x => x.Send(new TestScenarioCommand(0)))
                .Should().ThrowExactly<CommandNotRegisteredException>()
                .WithMessage($"Command '{nameof(TestScenarioCommand)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [TestCase(typeof(TimeoutException))]
        [TestCase(typeof(TaskCanceledException))]
        [TestCase(typeof(OperationCanceledException))]
        [TestCase(typeof(CommandDeferredException))]
        public void Send_throwsInterraptingKindOfException_thrownCommandDeferredException(Type exceptionType)
        {
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestSuccessFailureCommandHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.Send(new TestSuccessFailureCommand(exceptionType.AssemblyQualifiedName)))
                .Should().ThrowExactly<CommandDeferredException>()
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsCommandFailedException_thrownInvalidOperationException()
        {
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestScenarioCommandHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.Send(new TestScenarioCommand(1)))
                .Should().ThrowExactly<CommandFailedException>()
                .WithMessage("Command execution has failed.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsCommandFailedException_thrownCommandFailedException()
        {
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestScenarioCommandHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.Send(new TestScenarioCommand(2)))
                .Should().ThrowExactly<CommandFailedException>()
                .WithMessage("2")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsCommandFailedException_thrownCommandFailedExceptionWithInnerException()
        {
            using var fixture = new CommandClientFixtureBuilder()
                .AddRemote<TestScenarioCommandHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.Send(new TestScenarioCommand(3)))
                .Should().ThrowExactly<CommandFailedException>()
                .WithMessage("3")
                .WithInnerExceptionExactly<CommandFailedException>()
                .Which.InnerException?.Message.Should().Be("inner");
        }
    }
}