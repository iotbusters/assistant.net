using Assistant.Net.Abstractions;
using Assistant.Net.Logging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Assistant.Net.Logging.Tests;

public class LoggerExtensionsTests
{
    [Test]
    public void BeginPropertyScope_addsPropertyScopeValueToLoggerWithinContext()
    {
        var stringBuilder = new StringBuilder();
        Console.SetOut(new StringWriter(stringBuilder));
        using var provider = new ServiceCollection()
            .AddLogging(b => b.AddYamlConsole().SetMinimumLevel(LogLevel.Trace))
            .ReplaceSingleton<ISystemClock>(_ => new TestClock {UtcNow = testTime})
            .BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("1");

        using (logger.BeginPropertyScope("Property1", 1).AddPropertyScope("Property2", 2))
            logger.LogInformation("Test1");
        logger.LogInformation("Test2");

        Thread.Sleep(10);
        stringBuilder.ToString().Should().BeEquivalentTo(@$"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test1
Scopes:
  - Property1: 1
    Property2: 2

Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test2
");
    }

    [Test]
    public void BeginPropertyScope_addsPropertyScopeRuntimeValueToLoggerWithinContext()
    {
        var stringBuilder = new StringBuilder();
        Console.SetOut(new StringWriter(stringBuilder));
        using var provider = new ServiceCollection()
            .AddLogging(b => b.AddYamlConsole().SetMinimumLevel(LogLevel.Trace))
            .ReplaceSingleton<ISystemClock>(_ => new TestClock {UtcNow = testTime})
            .BuildServiceProvider();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("1");

        var i = 1;
        using (logger.BeginPropertyScope("Property1", () => i++).AddPropertyScope("Property2", () => i++))
        {
            logger.LogInformation("Test1");
            logger.LogInformation("Test2");
        }

        Thread.Sleep(10);
        stringBuilder.ToString().Should().BeEquivalentTo(@$"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test1
Scopes:
  - Property1: 1
    Property2: 2

Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: 1
Message: Test2
Scopes:
  - Property1: 3
    Property2: 4
");
    }

    private static readonly DateTimeOffset testTime = new(2000, 1, 1, 1, 1, 1, default);
}
