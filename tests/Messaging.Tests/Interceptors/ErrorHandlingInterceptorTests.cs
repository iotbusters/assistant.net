using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Tests.Mocks;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Interceptors;

public class ErrorHandlingInterceptorTests
{
    [Test]
    public async Task Intercept_returnsResponse()
    {
        var response = await Interceptor.Intercept((_,_) => Task.FromResult<object>(new TestResponse(false)), Message);

        response.Should().BeEquivalentTo(new TestResponse(false));
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionException()
    {
        await Interceptor.Awaiting(x => x.Intercept(Fail(new TestMessageExecutionException()), Message))
            .Should().ThrowAsync<TestMessageExecutionException>();
    }

    [TestCase(typeof(OperationCanceledException))]
    [TestCase(typeof(TaskCanceledException))]
    [TestCase(typeof(TimeoutException))]
    [TestCase(typeof(Exception))]
    public async Task Intercept_throws(Type exceptionType)
    {
        Options.ExposedExceptions.Add(exceptionType);

        try
        {
            await Interceptor.Intercept(Fail((Exception)Activator.CreateInstance(exceptionType)!), Message);
        }
        catch (Exception ex)
        {
            ex.Should().BeOfType(exceptionType);
            return;
        }
        Assert.Fail("Expected an exception to be thrown.");
    }

    [TestCase(typeof(OperationCanceledException))]
    [TestCase(typeof(TaskCanceledException))]
    [TestCase(typeof(TimeoutException))]
    [TestCase(typeof(Exception))]
    public async Task Intercept_throwsMessageFailedException_thrown(Type exceptionType)
    {
        await Interceptor.Awaiting(x => x.Intercept(Fail((Exception)Activator.CreateInstance(exceptionType)!), Message))
            .Should().ThrowAsync<MessageFailedException>();
    }

    [SetUp]
    public void Setup()
    {
        Options = new MessagingClientOptions();
        Interceptor = new ErrorHandlingInterceptor(new TestNamedOptions {Value = Options});
    }

    private MessagingClientOptions Options { get; set; } = null!;
    private IMessageInterceptor Interceptor { get; set; } = null!;
    private static TestMessage Message => new(0);

    private static Func<IMessage<object>, CancellationToken, Task<object>> Fail(Exception ex) => (_, _) => throw ex;
}
