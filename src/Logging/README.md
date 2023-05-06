# assistant.net.logging

Common .net logging extension.

## YAML console logging

YAML formatter logging is an extension to .net regular console logger.

```csharp
using var provider = new ServiceCollection()
    .AddLogging(b => b
      .AddYamlConsole()
      .AddPropertyScope("ApplicationName", () => Thread.CurrentThread.ManagedThreadId)
      .AddPropertyScope("Thread", () => "event-handler-1"))
    .BuildServiceProvider();

var logger = provider.GetRequiredService<ILogger<SomeService>>();
using var _ logger.BeginPropertyScope("RequestId", id);
logger.LogError(ex, "Querying timers: found arranged {TimerCount} timer(s).", count);
```

## Formatting sample

See the YAML formatter written log sample.

```yaml
Time: 2022-01-01 10:13:59.0192003
Level: Error
EventId: 0
Category: Assistant.Net.Internal.SomeService
Message: Querying timers: found arranged 1 timer(s).
State:
  MessageTemplate: Querying timers: found arranged {TimerCount} timer(s).
  TimerCount: 1
Exception:
  Message: Invalid operation.
  StackTrace: |
    at ConsoleApplication1.SomeObject.OtherMethod() in C:\ConsoleApplication1\SomeObject.cs:line 10
    at ConsoleApplication1.SomeObject..ctor() in C:\ConsoleApplication1\SomeObject.cs:line 20
  InnerException:
    Message: Invalid operation.
    StackTrace: |
      at ConsoleApplication1.SomeObject.OtherMethod() in C:\ConsoleApplication1\SomeObject.cs:line 10
      at ConsoleApplication1.SomeObject..ctor() in C:\ConsoleApplication1\SomeObject.cs:line 20
Scopes:
  - Name: ApplicationName
    Value: event-handler-1
  - Name: Thread
    Value: 24
  - Name: RequestId
    Value: 5793e715-6e50-4f84-9c9e-85be62de689c
```
