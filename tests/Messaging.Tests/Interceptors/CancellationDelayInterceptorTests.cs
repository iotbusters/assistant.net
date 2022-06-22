using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Interceptors;
using Assistant.Net.Messaging.Options;
using Assistant.Net.Messaging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Tests.Interceptors;

public class CancellationDelayInterceptorTests
{
    [Test]
    public async Task Intercept_delaysTaskCancellationFailure()
    {
        var cancelledSource = new CancellationTokenSource(0);
        var timer = Stopwatch.StartNew();

        await Interceptor.Awaiting(x => x.Intercept(async (_, token) =>
            {
                await Task.WhenAll(Task.Delay(Timeout.Infinite, token));
                timer.Stop();
                return Response;
            }, Message, cancelledSource.Token))
            .Should().ThrowAsync<OperationCanceledException>();
        timer.Elapsed.Should().BeGreaterThan(Options.CancellationDelay);
    }

    [Test]
    public async Task Intercept_noCancellation()
    {
        var source = new CancellationTokenSource();
        var timer = Stopwatch.StartNew();

        await Interceptor.Intercept((_, token) => Task.Run<object>(() =>
        {
            timer.Stop();
            return Response;
        }, token), Message, source.Token);

        timer.Elapsed.Should().BeLessThan(Options.CancellationDelay);
    }

    [Test]
    public async Task Intercept_delaysTaskCancellationSafely()
    {
        var cancelledSource = new CancellationTokenSource(0);
        var timer = Stopwatch.StartNew();

        await Interceptor.Intercept(async (_, token) =>
        {
            await Task.WhenAll(Task.Delay(Options.CancellationDelay, token));
            timer.Stop();
            return Response;
        }, Message, cancelledSource.Token);

        timer.Elapsed.Should().BeGreaterThan(Options.CancellationDelay);
    }

    [SetUp]
    public void Setup()
    {
        Options = new MessagingClientOptions {CancellationDelay = TimeSpan.FromSeconds(0.1)};
        var services = new ServiceCollection()
            .AddLogging()
            .AddTypeEncoder()
            .AddSingleton<INamedOptions<MessagingClientOptions>>(new TestNamedOptions {Value = Options})
            .AddSingleton<CancellationDelayInterceptor>()
            .AddTransient<CachingInterceptor>();
        Provider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown() => Provider.Dispose();

    private ServiceProvider Provider { get; set; } = default!;
    private IAbstractInterceptor Interceptor => Provider.GetRequiredService<CancellationDelayInterceptor>();
    private MessagingClientOptions Options { get; set; } = default!;
    private static TestMessage Message => new(0);
    private static TestResponse Response => new(false);
}
