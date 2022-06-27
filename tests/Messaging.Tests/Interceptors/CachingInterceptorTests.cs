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
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Interceptors;

public class CachingInterceptorTests
{
    [Test]
    public async Task Intercept_returnsResponseAndCaches()
    {
        var response = await Interceptor.Intercept((_, _) => Task.FromResult<object>(new TestResponse(false)), Message);

        response.Should().BeEquivalentTo(new TestResponse(false));
        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeEquivalentTo(CachingResult.OfValue(new TestResponse(false)));
    }

    [Test]
    public async Task Intercept_returnsResponseFromCache()
    {
        await Interceptor.Intercept((_, _) => Task.FromResult<object>(new TestResponse(false)), Message);

        var response = await Interceptor.Intercept((_, _) => Task.FromResult<object>(new TestResponse(true)), Message);

        response.Should().BeEquivalentTo(new TestResponse(false));
    }

    [Test]
    public async Task Intercept_returnsResponseAndDoesNotCache_INoneCaching()
    {
        var response = await Interceptor.Intercept((_, _) => Task.FromResult<object>(new TestResponse(false)), new TestMessage4());

        response.Should().BeEquivalentTo(new TestResponse(false));
        var cached = await Cache.GetOrDefault(new TestMessage4());
        cached.Should().BeNull();
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionAndCaches()
    {
        await Interceptor.Awaiting(x => x.Intercept(Fail(new TestMessageExecutionException()), Message))
            .Should().ThrowAsync<TestMessageExecutionException>();

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeEquivalentTo(
            CachingResult.OfException(new TestMessageExecutionException()),
            o => o.Excluding(m => m.Exception.StackTrace));
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionAndDoesNotCache_MessageDeferredException()
    {
        await Interceptor.Awaiting(x => x.Intercept(Fail(new MessageDeferredException()), Message))
            .Should().ThrowAsync<StorageException>().WithInnerException(typeof(MessageDeferredException));

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeNull();
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionAndDoesNotCache_TransientException()
    {
        Options.TransientExceptions.Add(typeof(ArgumentException));

        await Interceptor.Awaiting(x => x.Intercept(Fail(new ArgumentException()), Message))
            .Should().ThrowAsync<StorageException>().WithInnerException(typeof(ArgumentException));

        var cached = await Cache.GetOrDefault(Message);
        cached.Should().BeNull();
    }

    [Test]
    public async Task Intercept_throwsMessageExecutionExceptionFromCache()
    {
        await Cache.AddOrGet(Message, _ => new CachingExceptionResult(new TestMessageExecutionException()));

        await Interceptor.Awaiting(x => x.Intercept(Fail(new ArgumentException()), Message))
            .Should().ThrowAsync<TestMessageExecutionException>();
    }

    [SetUp]
    public void Setup()
    {
        Options = new MessagingClientOptions();
        var services = new ServiceCollection()
            .AddTransient<CachingInterceptor>()
            .AddSingleton<INamedOptions<MessagingClientOptions>>(new TestNamedOptions {Value = Options})
            .AddSystemClock()
            .AddStorage(b => b.AddLocal<IAbstractMessage, CachingResult>());
        Provider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown() => Provider.Dispose();

    private ServiceProvider Provider { get; set; } = default!;
    private IAbstractInterceptor Interceptor => Provider.GetRequiredService<CachingInterceptor>();
    private MessagingClientOptions Options { get; set; } = default!;
    private IStorage<IAbstractMessage, CachingResult> Cache => Provider.GetRequiredService<IStorage<IAbstractMessage, CachingResult>>();
    private static TestMessage Message => new(0);

    private static MessageInterceptor Fail(Exception ex) => (_, _) => throw ex;
}
