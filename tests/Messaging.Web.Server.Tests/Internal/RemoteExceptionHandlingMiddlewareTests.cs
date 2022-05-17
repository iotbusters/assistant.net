using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Web.Server.Tests.Fixtures;
using Assistant.Net.Messaging.Web.Server.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Server.Tests.Internal;

[Timeout(2000)]
public class RemoteExceptionHandlingMiddlewareTests
{
    [TestCase(typeof(TimeoutException))]
    [TestCase(typeof(TaskCanceledException))]
    [TestCase(typeof(OperationCanceledException))]
    [TestCase(typeof(MessageDeferredException))]
    public async Task Post_returnsStatusAccepted_thrownInterruptingKindOfException(Type exceptionType)
    {
        using var fixture = new MessagingClientFixtureBuilder()
            .AddWebHandler<TestFailMessageHandler>()
            .ClearInterceptors()
            .Create();

        var response = await fixture.Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));

        response.Should().BeEquivalentTo(new {StatusCode = HttpStatusCode.Accepted});
    }

    [Test]
    public async Task Post_returnsStatusNotFound_thrownMessageNotFoundException()
    {
        var exceptionType = typeof(MessageNotFoundException);
        using var fixture = new MessagingClientFixtureBuilder()
            .AddWebHandler<TestFailMessageHandler>()
            .Create();

        var response = await fixture.Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));

        response.Should().BeEquivalentTo(new {StatusCode = HttpStatusCode.NotFound});
        var responseObject = await fixture.ReadObject<MessageException>(response);
        responseObject.Should().BeOfType<MessageNotFoundException>();
    }

    [Test]
    public async Task Post_returnsStatusNotFound_thrownMessageNotRegisteredException()
    {
        var exceptionType = typeof(MessageNotRegisteredException);
        using var fixture = new MessagingClientFixtureBuilder()
            .AddWebHandler<TestFailMessageHandler>()
            .Create();

        var response = await fixture.Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));

        response.Should().BeEquivalentTo(new {StatusCode = HttpStatusCode.NotFound});
        var responseObject = await fixture.ReadObject<MessageException>(response);
        responseObject.Should().BeOfType<MessageNotRegisteredException>();
    }

    [Test]
    public async Task Post_returnsStatusBadRequest_thrownMessageContractException()
    {
        var exceptionType = typeof(MessageContractException);
        using var fixture = new MessagingClientFixtureBuilder()
            .AddWebHandler<TestFailMessageHandler>()
            .Create();

        var response = await fixture.Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));

        response.Should().BeEquivalentTo(new {StatusCode = HttpStatusCode.BadRequest});
        var responseObject = await fixture.ReadObject<MessageException>(response);
        responseObject.Should().BeOfType<MessageContractException>();
    }

    [Test]
    public async Task Post_returnsStatusInternalServerError_throwAnyOtherMessageException()
    {
        var exceptionType = typeof(TestMessageException);
        using var fixture = new MessagingClientFixtureBuilder()
            .AddWebHandler<TestFailMessageHandler>()
            .Create();

        var response = await fixture.Request(new TestFailMessage(exceptionType.AssemblyQualifiedName));

        response.Should().BeEquivalentTo(new {StatusCode = HttpStatusCode.InternalServerError});
        var responseObject = await fixture.ReadObject<MessageException>(response);
        responseObject.Should().BeOfType<TestMessageException>();
    }
}
