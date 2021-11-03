using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Web.Tests.Fixtures;
using Assistant.Net.Messaging.Web.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Tests
{
    [Timeout(2000)]
    public class ClientServerIntegrationTests
    {
        [Test]
        public async Task RequestObject_returnsResponse()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestScenarioMessageHandler>()
                .Create();

            var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

            response.Should().Be(new TestResponse(false));
        }

        [Test]
        public void RequestObject_throwsMessageNotRegisteredException_NoLocalHandler()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestSuccessFailureMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
                .Should().ThrowExactly<MessageNotRegisteredException>()
                .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void RequestObject_throwsMessageNotRegisteredException_NoRemoteHandler()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWeb<TestScenarioMessage>()
                .AddWebHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
                .Should().ThrowExactly<MessageNotRegisteredException>()
                .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [TestCase(typeof(TimeoutException))]
        [TestCase(typeof(TaskCanceledException))]
        [TestCase(typeof(OperationCanceledException))]
        [TestCase(typeof(MessageDeferredException))]
        public void RequestObject_throwsInterruptingKindOfException_thrownMessageDeferredException(Type exceptionType)
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestSuccessFailureMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestSuccessFailureMessage(exceptionType.AssemblyQualifiedName)))
                .Should().ThrowExactly<MessageDeferredException>()
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void RequestObject_throwsMessageFailedException_thrownInvalidOperationException()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(1)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("Message handling has failed.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void RequestObject_throwsMessageFailedException_thrownMessageFailedException()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(2)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("2")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void RequestObject_throwsMessageFailedException_thrownMessageFailedExceptionWithInnerException()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(3)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("3")
                .WithInnerExceptionExactly<MessageFailedException>()
                .Which.InnerException?.Message.Should().Be("inner");
        }

        [Test]
        public async Task PublishObject_returnsResponse()
        {
            var handler = new TestMessageHandler<TestScenarioMessage, TestResponse>(new TestResponse(false));
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler(handler)
                .Create();

            await fixture.Client.PublishObject(new TestScenarioMessage(0));

            handler.Message.Should().Be(new TestScenarioMessage(0));
        }

        [Test]
        public void PublishObject_throwsMessageNotRegisteredException_NoLocalHandler()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestSuccessFailureMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(0)))
                .Should().ThrowExactly<MessageNotRegisteredException>()
                .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void PublishObject_throwsMessageNotRegisteredException_NoRemoteHandler()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWeb<TestScenarioMessage>()
                .AddWebHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
                .Create();

            fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(0)))
                .Should().ThrowExactly<MessageNotRegisteredException>()
                .WithMessage($"Message '{nameof(TestScenarioMessage)}' wasn't registered.")
                .Which.InnerException.Should().BeNull();
        }

        [TestCase(typeof(TimeoutException))]
        [TestCase(typeof(TaskCanceledException))]
        [TestCase(typeof(OperationCanceledException))]
        [TestCase(typeof(MessageDeferredException))]
        public void PublishObject_throwsInterruptingKindOfException_thrownMessageDeferredException(Type exceptionType)
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestSuccessFailureMessageHandler>().ClearInterceptors()
                .Create();

            fixture.Client.Awaiting(x => x.PublishObject(new TestSuccessFailureMessage(exceptionType.AssemblyQualifiedName)))
                .Should().ThrowExactly<MessageDeferredException>()
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void PublishObject_throwsMessageFailedException_thrownInvalidOperationException()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(1)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("Message handling has failed.")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void PublishObject_throwsMessageFailedException_thrownMessageFailedException()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(2)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("2")
                .Which.InnerException.Should().BeNull();
        }

        [Test]
        public void PublishObject_throwsMessageFailedException_thrownMessageFailedExceptionWithInnerException()
        {
            using var fixture = new MessagingClientFixtureBuilder()
                .AddWebHandler<TestScenarioMessageHandler>()
                .Create();

            fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(3)))
                .Should().ThrowExactly<MessageFailedException>()
                .WithMessage("3")
                .WithInnerExceptionExactly<MessageFailedException>()
                .Which.InnerException?.Message.Should().Be("inner");
        }
    }
}
