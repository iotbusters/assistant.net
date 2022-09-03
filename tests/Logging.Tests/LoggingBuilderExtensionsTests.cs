using Assistant.Net.Abstractions;
using Assistant.Net.Logging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Logging.Tests;

public class LoggingBuilderExtensionsTests
{
    [Test]
    public async Task AddPropertyScope_addsRuntimeValueScope()
    {
        var stringBuilder = new StringBuilder();
        Console.SetOut(new StringWriter(stringBuilder));
        await using var provider = new ServiceCollection()
            .AddLogging(b => b
                .AddYamlConsole()
                .AddPropertyScope("Thread", () => Thread.CurrentThread.ManagedThreadId)
                .SetMinimumLevel(LogLevel.Trace))
            .ReplaceSingleton<ISystemClock>(_ => new TestClock {UtcNow = testTime})
            .BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("1");

        int thread1 = 0, thread2 = 0;
        await Task.WhenAll(
            Task.Run(() =>
            {
                thread1 = Thread.CurrentThread.ManagedThreadId;
                logger.LogInformation("Test1");
            }), Task.Run(() =>
            {
                thread2 = Thread.CurrentThread.ManagedThreadId;
                Thread.Sleep(5); // to guaranty order
                logger.LogInformation("Test2");
            }));

        await Task.Delay(10);
        stringBuilder.ToString()
            .Should().BeEquivalentTo(@$"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test1
Scopes:
  - Thread: {thread1}

Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test2
Scopes:
  - Thread: {thread2}
");
    }

    [Test]
    public async Task AddPropertyScope_addsDependencyInjectedValueScope()
    {
        var stringBuilder = new StringBuilder();
        Console.SetOut(new StringWriter(stringBuilder));
        await using var provider = new ServiceCollection()
            .AddLogging(b => b
                .AddYamlConsole()
                .AddPropertyScope("Value", p => p.GetRequiredService<ISystemClock>().UtcNow)
                .SetMinimumLevel(LogLevel.Trace))
            .ReplaceSingleton<ISystemClock>(_ => new TestClock {UtcNow = testTime})
            .BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("1");

        logger.LogInformation("Test1");

        await Task.Delay(10);
        stringBuilder.ToString()
            .Should().BeEquivalentTo(@$"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test1
Scopes:
  - Value: {testTime.ToString(null, CultureInfo.InvariantCulture)}
");
    }

    [Test]
    public async Task AddPropertyScope_addsStaticValueScope()
    {
        var stringBuilder = new StringBuilder();
        Console.SetOut(new StringWriter(stringBuilder));
        await using var provider = new ServiceCollection()
            .AddLogging(b => b
                .AddYamlConsole()
                .AddPropertyScope("Value", "1")
                .SetMinimumLevel(LogLevel.Trace))
            .ReplaceSingleton<ISystemClock>(_ => new TestClock {UtcNow = testTime})
            .BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("1");

        logger.LogInformation("Test1");

        await Task.Delay(10);
        stringBuilder.ToString()
            .Should().BeEquivalentTo(@$"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test1
Scopes:
  - Value: 1
");
    }

    [Test]
    public async Task AddPropertyScope_addsStaticDateTimeObjectScope()
    {
        var stringBuilder = new StringBuilder();
        Console.SetOut(new StringWriter(stringBuilder));
        await using var provider = new ServiceCollection()
            .AddLogging(b => b
                .AddYamlConsole()
                .AddPropertyScope("@Value", testTime)
                .SetMinimumLevel(LogLevel.Trace))
            .ReplaceSingleton<ISystemClock>(_ => new TestClock {UtcNow = testTime})
            .BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("1");

        logger.LogInformation("Test1");

        await Task.Delay(10);
        stringBuilder.ToString()
            .Should().BeEquivalentTo(@$"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test1
Scopes:
  - @Value: {testTime.ToString(null, CultureInfo.InvariantCulture)}
");
    }

    [Test]
    public async Task AddPropertyScope_addsStaticKeyValuePairObjectScope()
    {
        var stringBuilder = new StringBuilder();
        Console.SetOut(new StringWriter(stringBuilder));
        await using var provider = new ServiceCollection()
            .AddLogging(b => b
                .AddYamlConsole()
                .AddPropertyScope("@Value", new KeyValuePair<string, int>("1", 2))
                .SetMinimumLevel(LogLevel.Trace))
            .ReplaceSingleton<ISystemClock>(_ => new TestClock {UtcNow = testTime})
            .BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("1");

        logger.LogInformation("Test1");

        await Task.Delay(10);
        stringBuilder.ToString()
            .Should().BeEquivalentTo(@$"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test1
Scopes:
  - @Value:
      Key: 1
      Value: 2
");
    }

    [Test]
    public async Task AddPropertyScope_addsStaticKeyValuePairValueScope()
    {
        var stringBuilder = new StringBuilder();
        Console.SetOut(new StringWriter(stringBuilder));
        await using var provider = new ServiceCollection()
            .AddLogging(b => b
                .AddYamlConsole()
                .AddPropertyScope("Value", new KeyValuePair<string, int>("1", 2))
                .SetMinimumLevel(LogLevel.Trace))
            .ReplaceSingleton<ISystemClock>(_ => new TestClock {UtcNow = testTime})
            .BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("1");

        logger.LogInformation("Test1");

        await Task.Delay(10);
        stringBuilder.ToString()
            .Should().BeEquivalentTo(@$"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test1
Scopes:
  - Value: [1, 2]
");
    }

    private static readonly DateTimeOffset testTime = new(2000, 1, 1, 1, 1, 1, default);
}
