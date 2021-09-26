using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Integration.Tests.Fixtures;
using Assistant.Net.Messaging.Integration.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Integration.Tests
{
    [Timeout(2000)]
    public class ClientServerIntegrationTests
    {
        [Test]
        public async Task Send_returnsResponse()
        {
            using var fixture = new MessageClientFixtureBuilder()
                .AddRemote<TestScenarioMessageHandler>()
                .Create();

            var response = await fixture.Client.SendObject(new TestScenarioMessage(0));

            response.Should().Be(new TestResponse(false));
        }

        [Test]
        public void Send_throwsMessageNotRegisteredException_NoLocalHandler()
        {
            using var fixture = new MessageClientFixtureBuilder()
                .AddRemote<TestSuccessFailureMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.SendObject(new TestScenarioMessage(0)))
                .Should().ThrowExactly<MessageNotRegisteredException>()
                .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsMessageNotRegisteredException_NoRemoteHandler()
        {
            using var fixture = new MessageClientFixtureBuilder()
                .AddRemoteMessageRegistrationOnly<TestScenarioMessage>()
                .AddRemote<TestSuccessFailureMessageHandler>()// to have at least one handler configured
                .Create();

            fixture.Client.Awaiting(x => x.SendObject(new TestScenarioMessage(0)))
                .Should().ThrowExactly<MessageNotRegisteredException>()
                .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [TestCase(typeof(TimeoutException))]
        [TestCase(typeof(TaskCanceledException))]
        [TestCase(typeof(OperationCanceledException))]
        [TestCase(typeof(MessageDeferredException))]
        public void Send_throwsInterruptingKindOfException_thrownMessageDeferredException(Type exceptionType)
        {
            using var fixture = new MessageClientFixtureBuilder()
                .AddRemote<TestSuccessFailureMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.SendObject(new TestSuccessFailureMessage(exceptionType.AssemblyQualifiedName)))
                .Should().ThrowExactly<MessageDeferredException>()
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsMessageFailedException_thrownInvalidOperationException()
        {
            using var fixture = new MessageClientFixtureBuilder()
                .AddRemote<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.SendObject(new TestScenarioMessage(1)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("Message handling has failed.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsMessageFailedException_thrownMessageFailedException()
        {
            using var fixture = new MessageClientFixtureBuilder()
                .AddRemote<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.SendObject(new TestScenarioMessage(2)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("2")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void Send_throwsMessageFailedException_thrownMessageFailedExceptionWithInnerException()
        {
            using var fixture = new MessageClientFixtureBuilder()
                .AddRemote<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.SendObject(new TestScenarioMessage(3)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("3")
                .WithInnerExceptionExactly<MessageFailedException>()
                .Which.InnerException?.Message.Should().Be("inner");
        }
    }
}