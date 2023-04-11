using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Interceptors;

public class ErrorHandlingInterceptorTests
{
    [Test]
    public async Task Intercept_returnsResponse_request()
    {
        var response = await Interceptor.Intercept(SuccessRequest(new TestResponse(false)), Message, default);

        response.Should().BeEquivalentTo(new TestResponse(false));
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionException_requestAndThrownExposedException()
    {
        Options.ExposedExceptions.Add(typeof(MessageException));
        await Interceptor.Awaiting(x => x.Intercept(FailRequest(new TestMessageExecutionException()), Message, default))
            .Should().ThrowAsync<TestMessageExecutionException>();
    }

    [TestCase(typeof(OperationCanceledException))]
    [TestCase(typeof(TaskCanceledException))]
    [TestCase(typeof(TimeoutException))]
    [TestCase(typeof(Exception))]
    public async Task Intercept_throws_requestAndThrown(Type exceptionType)
    {
        Options.ExposedExceptions.Add(exceptionType);

        try
        {
            await Interceptor.Intercept(FailRequest((Exception)Activator.CreateInstance(exceptionType)!), Message, default);
        }
        catch (Exception ex) // ThrowAsync() doesn't support System.Type argument
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
    public async Task Intercept_throwsMessageFailedException_requestAndThrown(Type exceptionType) =>
        await Interceptor.Awaiting(x => x.Intercept(FailRequest((Exception)Activator.CreateInstance(exceptionType)!), Message, default))
            .Should().ThrowAsync<MessageFailedException>();

    [Test]
    public async Task Intercept_returnsResponse_publish() =>
        await Interceptor.Awaiting(x => x.Intercept(SuccessPublish(), Message, default))
            .Should().NotThrowAsync();

    [Test]
    public async Task Intercept_throwsMessageExecutionException_publishAndThrownExposedException()
    {
        Options.ExposedExceptions.Add(typeof(MessageException));
        await Interceptor.Awaiting(x => x.Intercept(FailPublish(new TestMessageExecutionException()), Message, default))
            .Should().ThrowAsync<TestMessageExecutionException>();
    }

    [TestCase(typeof(OperationCanceledException))]
    [TestCase(typeof(TaskCanceledException))]
    [TestCase(typeof(TimeoutException))]
    [TestCase(typeof(Exception))]
    public async Task Intercept_throws_publishAndThrown(Type exceptionType)
    {
        Options.ExposedExceptions.Add(exceptionType);

        try
        {
            await Interceptor.Intercept(FailPublish((Exception)Activator.CreateInstance(exceptionType)!), Message, default);
        }
        catch (Exception ex) // ThrowAsync() doesn't support System.Type argument
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
    public async Task Intercept_throwsMessageFailedException_publishAndThrown(Type exceptionType) =>
        await Interceptor.Awaiting(x => x.Intercept(FailPublish((Exception)Activator.CreateInstance(exceptionType)!), Message, default))
            .Should().ThrowAsync<MessageFailedException>();

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Options = new();
        var services = new ServiceCollection()
            .AddTypeEncoder()
            .AddTransient<INamedOptions<MessagingClientOptions>>(_=> new TestNamedOptions(() => Options))
            .AddTransient<ErrorHandlingInterceptor>();
        Provider = services.BuildServiceProvider();
    }

    [SetUp]
    public void Setup() => Options = new();

    [OneTimeTearDown]
    public void OneTimeTearDown() => Provider.Dispose();

    private ServiceProvider Provider { get; set; } = default!;
    private MessagingClientOptions Options { get; set; } = default!;
    private ErrorHandlingInterceptor Interceptor => Provider.GetRequiredService<ErrorHandlingInterceptor>();
    private static TestMessage Message => new(0);

    private static RequestMessageHandler SuccessRequest(object response) => (_, _) => ValueTask.FromResult(response);
    private static PublishMessageHandler SuccessPublish() => (_, _) => ValueTask.CompletedTask;
    private static RequestMessageHandler FailRequest(Exception ex) => (_, _) => throw ex;
    private static PublishMessageHandler FailPublish(Exception ex) => (_, _) => throw ex;
}
