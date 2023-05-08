using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Web.Tests.Fixtures;
using Assistant.Net.Messaging.Web.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Tests;

[Timeout(3000)]
public class ClientServerIntegrationTests
{
    [Test]
    public async Task RequestObject_returnsResponse()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var response = await fixture.Client.RequestObject(new TestScenarioMessage(0));

        response.Should().Be(new TestResponse(false));
    }

    [Test]
    public async Task Send_returnsAnotherResponse_serverSideHandlerChanged()
    {
        // global arrange
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
            .ClearInterceptors()
            .Create();

        // arrange 1
        fixture.ReplaceHandlers(new TestMessageHandler<TestScenarioMessage, TestResponse>(new(true)));

        // act 1
        var response1 = await fixture.Client.RequestObject(new TestScenarioMessage(1));

        // assert 1
        response1.Should().BeEquivalentTo(new TestResponse(true));

        // arrange 2
        fixture.ReplaceHandlers(new TestMessageHandler<TestScenarioMessage, TestResponse>(new(false)));

        // act 2
        var response2 = await fixture.Client.RequestObject(new TestScenarioMessage(2));

        // assert 2
        response2.Should().BeEquivalentTo(new TestResponse(false));
    }

    [Test]
    public async Task RequestObject_throwsMessageNotRegisteredException_NoLocalHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestSuccessFailureMessageHandler>()
            .Create();

        var ex = await fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
            .Should().ThrowExactlyAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{typeof(TestScenarioMessage)}' wasn't registered.");
        ex.Which.InnerException.Should().BeNull();
    }

    [Test]
    public async Task RequestObject_throwsMessageNotRegisteredException_NoRemoteHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddWeb<TestScenarioMessage>()
            .AddHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
            .Create();

        var ex = await fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(0)))
            .Should().ThrowExactlyAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{typeof(TestScenarioMessage)}' wasn't registered.");
        ex.Which.InnerException.Should().BeNull();
    }

    [TestCase(typeof(TimeoutException))]
    [TestCase(typeof(TaskCanceledException))]
    [TestCase(typeof(OperationCanceledException))]
    [TestCase(typeof(MessageDeferredException))]
    public async Task RequestObject_throwsInterruptingKindOfException_thrownMessageDeferredException(Type exceptionType)
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestSuccessFailureMessageHandler>()
            .ClearInterceptors()
            .Create();

        var ex = await fixture.Client.Awaiting(x => x.RequestObject(new TestSuccessFailureMessage(exceptionType.AssemblyQualifiedName)))
            .Should().ThrowExactlyAsync<MessageDeferredException>();
        ex.Which.InnerException.Should().BeNull();
    }

    [Test]
    public async Task RequestObject_throwsMessageFailedException_thrownInvalidOperationException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var ex = await fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(1)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("Message handling has failed.");
        ex.Which.InnerException.Should().BeNull();
    }

    [Test]
    public async Task RequestObject_throwsMessageFailedException_thrownMessageFailedException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var ex = await fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(2)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("2");
        ex.Which.InnerException.Should().BeNull();
    }

    [Test]
    public async Task RequestObject_throwsMessageFailedException_thrownMessageFailedExceptionWithInnerException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var ex = await fixture.Client.Awaiting(x => x.RequestObject(new TestScenarioMessage(3)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("3");
        ex
            .Which.InnerException.Should().BeOfType<MessageFailedException>()
            .Which.Message.Should().Be("inner");
    }

    [Test]
    public async Task PublishObject_returnsResponse()
    {
        var handler = new TestMessageHandler<TestScenarioMessage, TestResponse>(new(false));
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler(handler)
            .Create();

        await fixture.Client.PublishObject(new TestScenarioMessage(0));

        handler.Message.Should().Be(new TestScenarioMessage(0));
    }

    [Test]
    public async Task PublishObject_throwsMessageNotRegisteredException_NoLocalHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestSuccessFailureMessageHandler>()
            .Create();

        await fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(0)))
            .Should().ThrowExactlyAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{typeof(TestScenarioMessage)}' wasn't registered.");
    }

    [Test]
    public async Task PublishObject_throwsMessageNotRegisteredException_NoRemoteHandler()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddWeb<TestScenarioMessage>()
            .AddHandler<TestSuccessFailureMessageHandler>()// to have at least one handler configured
            .Create();

        await fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(0)))
            .Should().ThrowExactlyAsync<MessageNotRegisteredException>()
            .WithMessage($"Message '{typeof(TestScenarioMessage)}' wasn't registered.");
    }

    [TestCase(typeof(TimeoutException))]
    [TestCase(typeof(TaskCanceledException))]
    [TestCase(typeof(OperationCanceledException))]
    [TestCase(typeof(MessageDeferredException))]
    public async Task PublishObject_throwsInterruptingKindOfException_thrownNoException(Type exceptionType)
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestSuccessFailureMessageHandler>()
            .ClearInterceptors()
            .Create();

        await fixture.Client.Awaiting(x => x.PublishObject(new TestSuccessFailureMessage(exceptionType.AssemblyQualifiedName)))
            .Should().NotThrowAsync();
    }

    [Test]
    public async Task PublishObject_throwsMessageFailedException_thrownInvalidOperationException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var ex = await fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(1)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("Message handling has failed.");
        ex.Which.InnerException.Should().BeNull();
    }

    [Test]
    public async Task PublishObject_throwsMessageFailedException_thrownMessageFailedException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var ex = await fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(2)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("2");
        ex.Which.InnerException.Should().BeNull();
    }

    [Test]
    public async Task PublishObject_throwsMessageFailedException_thrownMessageFailedExceptionWithInnerException()
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestScenarioMessageHandler>()
            .Create();

        var ex = await fixture.Client.Awaiting(x => x.PublishObject(new TestScenarioMessage(3)))
            .Should().ThrowExactlyAsync<MessageFailedException>()
            .WithMessage("3");
        ex.WithInnerExceptionExactly<MessageFailedException>()
            .WithMessage("inner");
    }
}
