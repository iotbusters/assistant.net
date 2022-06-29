using Assistant.Net.Abstractions;
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
    public async Task Intercept_delaysCancellation_requestAndLongRunningOperation()
    {
        var cancelledSource = new CancellationTokenSource(0);
        var timer = Stopwatch.StartNew();

        await Interceptor.Intercept(async (_, token) =>
        {
            await Task.WhenAny(Task.Delay(Timeout.Infinite, token));
            return Response;
        }, Message, cancelledSource.Token);
        timer.Stop();

        timer.Elapsed.Should().BeGreaterThan(Options.CancellationDelay * Approximation);
    }

    [Test]
    public async Task Intercept_doesNotDelay_requestAndNoCancellation()
    {
        var source = new CancellationTokenSource();
        var timer = Stopwatch.StartNew();

        await Interceptor.Intercept((_, _) => ValueTask.FromResult<object>(Response), Message, source.Token);
        timer.Stop();

        timer.Elapsed.Should().BeLessThan(Options.CancellationDelay * Approximation);
    }

    [Test]
    public async Task Intercept_delaysCancellation_publishAndLongRunningOperation()
    {
        var cancelledSource = new CancellationTokenSource(0);
        var timer = Stopwatch.StartNew();

        await Interceptor.Intercept(async (_, token) =>
        {
            await Task.WhenAny(Task.Delay(Timeout.Infinite, token));
        }, Message, cancelledSource.Token);
        timer.Stop();

        timer.Elapsed.Should().BeGreaterThan(Options.CancellationDelay * Approximation);
    }

    [Test]
    public async Task Intercept_doesNotDelay_publishAndNoCancellation()
    {
        var source = new CancellationTokenSource();
        var timer = Stopwatch.StartNew();

        await Interceptor.Intercept((_, _) => ValueTask.CompletedTask, Message, source.Token);
        timer.Stop();

        timer.Elapsed.Should().BeLessThan(Options.CancellationDelay * Approximation);
    }

    [OneTimeSetUp]
    public void Setup()
    {
        Options = new MessagingClientOptions {CancellationDelay = TimeSpan.FromSeconds(0.1)};
        var services = new ServiceCollection()
            .AddLogging()
            .AddTypeEncoder()
            .AddSingleton<INamedOptions<MessagingClientOptions>>(new TestNamedOptions(Options))
            .AddSingleton<CancellationDelayInterceptor>()
            .AddTransient<CachingInterceptor>();
        Provider = services.BuildServiceProvider();
    }

    [OneTimeTearDown]
    public void TearDown() => Provider.Dispose();

    private const double Approximation = 0.9;

    private ServiceProvider Provider { get; set; } = default!;
    private CancellationDelayInterceptor Interceptor => Provider.GetRequiredService<CancellationDelayInterceptor>();
    private MessagingClientOptions Options { get; set; } = default!;
    private static TestMessage Message => new(0);
    private static TestResponse Response => new(false);
}
