using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Web.Tests.Fixtures;
using Assistant.Net.Messaging.Web.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Tests.Internal;

[Timeout(2000)]
public class ExceptionHandlingMiddlewareTests
{
    [TestCase(typeof(TimeoutException))]
    [TestCase(typeof(TaskCanceledException))]
    [TestCase(typeof(OperationCanceledException))]
    [TestCase(typeof(MessageDeferredException))]
    public async Task Post_returnsStatusAccepted_thrownInterruptingKindOfException(Type exceptionType)
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestFailMessageHandler>()
            .ClearInterceptors()
            .Create();

        await fixture.Awaiting(x => x.WebRequest(new TestFailMessage(exceptionType.AssemblyQualifiedName)))
            .Should().ThrowAsync<MessageDeferredException>();
    }

    [Test]
    public async Task Post_returnsStatusNotFound_thrownMessageNotFoundException()
    {
        var exceptionType = typeof(MessageNotFoundException);
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestFailMessageHandler>()
            .Create();

        await fixture.Awaiting(x => x.WebRequest(new TestFailMessage(exceptionType.AssemblyQualifiedName)))
            .Should().ThrowAsync<MessageNotFoundException>();
    }

    [Test]
    public async Task Post_returnsStatusNotFound_thrownMessageNotRegisteredException()
    {
        var exceptionType = typeof(MessageNotRegisteredException);
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestFailMessageHandler>()
            .Create();

        await fixture.Awaiting(x => x.WebRequest(new TestFailMessage(exceptionType.AssemblyQualifiedName)))
            .Should().ThrowAsync<MessageNotRegisteredException>();
    }

    [Test]
    public async Task Post_returnsStatusBadRequest_thrownMessageContractException()
    {
        var exceptionType = typeof(MessageContractException);
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestFailMessageHandler>()
            .Create();

        await fixture.Awaiting(x => x.WebRequest(new TestFailMessage(exceptionType.AssemblyQualifiedName)))
            .Should().ThrowAsync<MessageContractException>();
    }

    [Test]
    public async Task Post_returnsStatusInternalServerError_throwAnyOtherMessageException()
    {
        var exceptionType = typeof(TestMessageException);
        using var fixture = new MessagingClientFixtureBuilder()
            .AddHandler<TestFailMessageHandler>()
            .Create();

        await fixture.Awaiting(x => x.WebRequest(new TestFailMessage(exceptionType.AssemblyQualifiedName)))
            .Should().ThrowAsync<TestMessageException>();
    }
}
