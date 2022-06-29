using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Tests.Mocks;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Interceptors;

public class CachingInterceptorTests
{
    [Test]
    public async Task Intercept_returnsResponseAndCaches_request()
    {
        var response = await Interceptor.Intercept(SuccessRequest(new TestResponse(false)), Message, default);

        response.Should().BeEquivalentTo(new TestResponse(false));
        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeEquivalentTo(CachingResult.OfValue(new TestResponse(false)));
    }

    [Test]
    public async Task Intercept_returnsResponseFromCache_request()
    {
        await Interceptor.Intercept(SuccessRequest(new TestResponse(false)), Message, default);

        var response = await Interceptor.Intercept((_, _) => ValueTask.FromResult<object>(new TestResponse(true)), Message, default);

        response.Should().BeEquivalentTo(new TestResponse(false));
    }

    [Test]
    public async Task Intercept_returnsResponseAndDoesNotCache_requestAndINoneCaching()
    {
        var response = await Interceptor.Intercept(SuccessRequest(new TestResponse(false)), new TestMessage4(), default);

        response.Should().BeEquivalentTo(new TestResponse(false));
        var cached = await Cache.GetOrDefault(new TestMessage4());
        cached.Should().BeNull();
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionAndCaches_request()
    {
        await Interceptor.Awaiting(x => x.Intercept(FailRequest(new TestMessageExecutionException()), Message, default))
            .Should().ThrowAsync<TestMessageExecutionException>();

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeEquivalentTo(
            CachingResult.OfException(new TestMessageExecutionException()),
            o => o.Excluding(m => m.Exception.StackTrace));
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionAndDoesNotCache_requestAndMessageDeferredException()
    {
        await Interceptor.Awaiting(x => x.Intercept(FailRequest(new MessageDeferredException()), Message, default))
            .Should().ThrowAsync<StorageException>().WithInnerException(typeof(MessageDeferredException));

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeNull();
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionAndDoesNotCache_requestAndTransientException()
    {
        Options.TransientExceptions.Add(typeof(ArgumentException));

        await Interceptor.Awaiting(x => x.Intercept(FailRequest(new ArgumentException()), Message, default))
            .Should().ThrowAsync<StorageException>().WithInnerException(typeof(ArgumentException));

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeNull();
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionFromCache_request()
    {
        await Cache.AddOrGet(Message, _ => new CachingExceptionResult(new TestMessageExecutionException()));

        await Interceptor.Awaiting(x => x.Intercept(FailRequest(new ArgumentException()), Message, default))
            .Should().ThrowAsync<TestMessageExecutionException>();
    }

    [Test]
    public async Task Intercept_caches_publish()
    {
        await Interceptor.Intercept(SuccessPublish(), Message, default);

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeEquivalentTo(CachingResult.OfValue(Nothing.Instance));
    }

    [Test]
    public async Task Intercept_doesNotCache_publishAndINoneCaching()
    {
        await Interceptor.Intercept(SuccessPublish(), new TestMessage4(), default);

        var cached = await Cache.GetOrDefault(new TestMessage4());
        cached.Should().BeNull();
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionAndCaches_publish()
    {
        await Interceptor.Awaiting(x => x.Intercept(FailPublish(new TestMessageExecutionException()), Message, default))
            .Should().ThrowAsync<TestMessageExecutionException>();

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeEquivalentTo(
            CachingResult.OfException(new TestMessageExecutionException()),
            o => o.Excluding(m => m.Exception.StackTrace));
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionAndDoesNotCache_publishAndMessageDeferredException()
    {
        await Interceptor.Awaiting(x => x.Intercept(FailPublish(new MessageDeferredException()), Message, default))
            .Should().ThrowAsync<StorageException>().WithInnerException(typeof(MessageDeferredException));

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeNull();
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionAndDoesNotCache_publishAndTransientException()
    {
        Options.TransientExceptions.Add(typeof(ArgumentException));

        await Interceptor.Awaiting(x => x.Intercept(FailPublish(new ArgumentException()), Message, default))
            .Should().ThrowAsync<StorageException>().WithInnerException(typeof(ArgumentException));

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeNull();
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionFromCache_Publish()
    {
        await Cache.AddOrGet(Message, _ => new CachingExceptionResult(new TestMessageExecutionException()));

        await Interceptor.Awaiting(x => x.Intercept(FailPublish(new ArgumentException()), Message, default))
            .Should().ThrowAsync<TestMessageExecutionException>();
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Options = new MessagingClientOptions();
        var services = new ServiceCollection()
            .AddTransient<CachingInterceptor>()
            .AddSingleton<INamedOptions<MessagingClientOptions>>(new TestNamedOptions(Options))
            .AddSystemClock()
            .AddStorage(b => b.AddLocal<IAbstractMessage, CachingResult>());
        Provider = services.BuildServiceProvider();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() => Provider.Dispose();

    [TearDown]
    public async Task TearDown()
    {
        var keys = await Cache.GetKeys().AsEnumerableAsync();
        await Task.WhenAll(keys.Select(x => Cache.TryRemove(x)));
    }

    private ServiceProvider Provider { get; set; } = default!;
    private CachingInterceptor Interceptor => Provider.GetRequiredService<CachingInterceptor>();
    private MessagingClientOptions Options { get; set; } = default!;
    private IAdminStorage<IAbstractMessage, CachingResult> Cache => Provider.GetRequiredService<IAdminStorage<IAbstractMessage, CachingResult>>();
    private static TestMessage Message => new(0);

    private static RequestMessageHandler SuccessRequest(object response) => (_, _) => ValueTask.FromResult(response);
    private static PublishMessageHandler SuccessPublish() => (_, _) => ValueTask.CompletedTask;
    private static RequestMessageHandler FailRequest(Exception ex) => (_, _) => throw ex;
    private static PublishMessageHandler FailPublish(Exception ex) => (_, _) => throw ex;
}
