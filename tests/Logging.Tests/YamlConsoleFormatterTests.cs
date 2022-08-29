using Assistant.Net.Abstractions;
using Assistant.Net.Logging.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Assistant.Net.Logging.Tests
{
    public class YamlConsoleFormatterTests
    {
        [TestCase(LogLevel.Trace)]
        [TestCase(LogLevel.Debug)]
        [TestCase(LogLevel.Information)]
        [TestCase(LogLevel.Warning)]
        [TestCase(LogLevel.Error)]
        [TestCase(LogLevel.Critical)]
        public void Write_writesToConsole_logLevel(LogLevel level)
        {
            logger.Log(level, "Message 1 and 2.");

            Thread.Sleep(10);
            stringBuilder.ToString().Should().Be($@"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: {level}
EventId: 0
Category: Assistant.Net.Logging.Tests.YamlConsoleFormatterTests
Message: Message 1 and 2.
");
        }

        [Test]
        public void Write_writesToConsole_stateProperties()
        {
            logger.LogInformation("Message {Property1} and {Property2}.", "1", 2);

            Thread.Sleep(10);
            stringBuilder.ToString().Should().Be($@"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: Assistant.Net.Logging.Tests.YamlConsoleFormatterTests
Message: Message 1 and 2.
State:
  Property1: 1
  Property2: 2
  MessageTemplate: Message {{Property1}} and {{Property2}}.
");
        }

        [Test]
        public void Write_writesToConsole_stateStructured()
        {
            logger.LogInformation("Message {Object}.", new TestClass {Key = "1", Value = "2"});

            Thread.Sleep(10);
            stringBuilder.ToString().Should().Be($@"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: Assistant.Net.Logging.Tests.YamlConsoleFormatterTests
Message: Message Assistant.Net.Logging.Tests.Mocks.TestClass.
State:
  Object:
    Type: Assistant.Net.Logging.Tests.Mocks.TestClass
    Key: 1
    Value: 2
  MessageTemplate: Message {{Object}}.
");
        }

        [Test]
        public void Write_writesToConsole_stateArray()
        {
            logger.LogInformation("Messages {Array}.", (object)new[] {"1", "2"});

            Thread.Sleep(10);
            stringBuilder.ToString().Should().Be($@"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: Assistant.Net.Logging.Tests.YamlConsoleFormatterTests
Message: Messages 1, 2.
State:
  Array:
    - 1
    - 2
  MessageTemplate: Messages {{Array}}.
");
        }

        [Test]
        public void Write_writesToConsole_exception()
        {
            var exception = new Exception("Error.", new Exception("Inner error."));

            logger.LogInformation(exception, "Message 1 and 2.");

            Thread.Sleep(10);
            stringBuilder.ToString().Should().Be($@"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: Assistant.Net.Logging.Tests.YamlConsoleFormatterTests
Message: Message 1 and 2.
Exception:
  Type: System.Exception
  Message: Error.
  InnerException:
    Type: System.Exception
    Message: Inner error.
");
        }

        [Test]
        public void Write_writesToConsole_scopes()
        {
            using var _ = logger.BeginScope("Test-Property");
            using var _1 = logger.BeginScope(new KeyValuePair<string, string>("1", "2"));
            using var _2 = logger.BeginScope(new TimeSpan(1, 1, 1, 1, 111));
            using var _3 = logger.BeginScope(new TestStruct {Key = "1", Value = "2"});
            using var _4 = logger.BeginScope(new[] {"1", "2"});
            using var _5 = logger.BeginScope((Id: "1", Value: "2"));
            using var _6 = logger.BeginScope(new TestClass {Key = "1", Value = "2"});
            logger.LogInformation("Message 1 and 2.");

            Thread.Sleep(10);
            stringBuilder.ToString().Should().Be($@"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: Assistant.Net.Logging.Tests.YamlConsoleFormatterTests
Message: Message 1 and 2.
Scopes:
  - Test-Property
  - [1, 2]
  - 1.01:01:01.1110000
  - Type: Assistant.Net.Logging.Tests.Mocks.TestStruct
    Key: 1
    Value: 2
  - - 1
    - 2
  - (1, 2)
  - Type: Assistant.Net.Logging.Tests.Mocks.TestClass
    Key: 1
    Value: 2
");
        }

        [Test]
        public void Write_writesToConsole_TimestampFormatIsUpdated()
        {
            options.TimestampFormat = "yyyy";

            logger.LogInformation("Message 1 and 2.");

            Thread.Sleep(10);
            stringBuilder.ToString().Should().Be($@"
Timestamp: {testTime.UtcDateTime:yyyy}
LogLevel: Information
EventId: 0
Category: Assistant.Net.Logging.Tests.YamlConsoleFormatterTests
Message: Message 1 and 2.
");
        }

        [Test]
        public void Write_writesToConsole_UseUtcTimestampIsFalse()
        {
            options.UseUtcTimestamp = false;

            logger.LogInformation("Message 1 and 2.");

            Thread.Sleep(10);
            stringBuilder.ToString().Should().Be($@"
Timestamp: {testTime.LocalDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: Assistant.Net.Logging.Tests.YamlConsoleFormatterTests
Message: Message 1 and 2.
");
        }

        [Test]
        public void Write_writesToConsole_IncludeScopesIsFalse()
        {
            options.IncludeScopes = false;

            using var _ = logger.BeginScope(new KeyValuePair<string, string>("1", "2"));
            logger.LogInformation("Message 1 and 2.");

            Thread.Sleep(10);
            stringBuilder.ToString().Should().Be($@"
Timestamp: {testTime.UtcDateTime:yyyy-MM-dd hh:mm:ss.fff}
LogLevel: Information
EventId: 0
Category: Assistant.Net.Logging.Tests.YamlConsoleFormatterTests
Message: Message 1 and 2.
");
        }

        private static readonly DateTimeOffset testTime = new(2000, 1, 1, 1, 1, 1, default);

        private StringBuilder stringBuilder = null!;
        private ServiceProvider provider = null!;
        private ILogger<YamlConsoleFormatterTests> logger = null!;

        private ConsoleFormatterOptions options = null!;

        [SetUp]
        public void Setup()
        {
            stringBuilder = new StringBuilder();
            Console.SetOut(new StringWriter(stringBuilder));

            options = new()
            {
                IncludeScopes = true,
                TimestampFormat = "yyyy-MM-dd hh:mm:ss.fff",
                UseUtcTimestamp = true
            };

            provider = new ServiceCollection()
                .AddLogging(b => b.AddYamlConsole().SetMinimumLevel(LogLevel.Trace))
                .ReplaceSingleton<IOptionsMonitor<ConsoleFormatterOptions>>(_ =>
                    new TestOptionsMonitor<ConsoleFormatterOptions>(options))
                .ReplaceSingleton<ISystemClock>(_ => new TestClock {UtcNow = testTime})
                .BuildServiceProvider();
            logger = provider.GetRequiredService<ILogger<YamlConsoleFormatterTests>>();
        }

        [TearDown]
        public void TearDown() => provider.Dispose();
    }
}
