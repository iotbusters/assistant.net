using Assistant.Net.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assistant.Net.Logging.Internal;

internal sealed class YamlConsoleFormatter : ConsoleFormatter, IDisposable
{
    private readonly ISystemClock clock;
    private ConsoleFormatterOptions formatterOptions;
    private readonly IDisposable disposable;

    public YamlConsoleFormatter(
        IOptionsMonitor<ConsoleFormatterOptions> options,
        ISystemClock clock) : base(ConsoleFormatterNames.Yaml)
    {
        this.formatterOptions = options.CurrentValue;
        this.disposable = options.OnChange(o => formatterOptions = o);
        this.clock = clock;
    }

    void IDisposable.Dispose() => disposable.Dispose();

    // Log sample:
    //
    // Time: 2022-01-01 10:13:59.0192003
    // Level: Debug
    // EventId: 0
    // Category: Assistant.Net.Internal.SomeService
    // Message: Querying timers: found arranged 1 timer(s).
    // State:
    //   MessageTemplate: Querying timers: found arranged {TimerCount} timer(s).
    //   TimerCount: 1
    // Exception:
    //   Message: Invalid operation.
    //   StackTrace:
    //     - at ConsoleApplication1.SomeObject.OtherMethod() in C:\ConsoleApplication1\SomeObject.cs:line 24
    //     - at ConsoleApplication1.SomeObject..ctor() in C:\ConsoleApplication1\SomeObject.cs:line 14
    //   InnerException:
    //     Message: Invalid operation.
    //     StackTrace:
    //       - at ConsoleApplication1.SomeObject.OtherMethod() in C:\ConsoleApplication1\SomeObject.cs:line 24
    //       - at ConsoleApplication1.SomeObject..ctor() in C:\ConsoleApplication1\SomeObject.cs:line 14
    // Scopes:
    //   - Name: ApplicationName
    //     Value: eventhndler1
    //   - Name: Thread
    //     Value: 24
    //   - Name: CorrelationId
    //     Value: 5793e715-6e50-4f84-9c9e-85be62de689c
    //   - Name: User
    //     Value: ababaga@gmail.com
    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter writer)
    {
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (logEntry.Exception == null && message == null)
            return;

        var dateTimeOffset = clock.UtcNow;
        var dateTime = formatterOptions.UseUtcTimestamp ? dateTimeOffset.DateTime : dateTimeOffset.LocalDateTime;
        var dateTimeString = dateTime.ToString(formatterOptions.TimestampFormat);

        var list = new List<IItem>
        {
            new PropertyItem("Timestamp", dateTimeString),
            new PropertyItem("LogLevel", logEntry.LogLevel),
            new PropertyItem("EventId", logEntry.EventId.Id),
            new PropertyItem("EventName", logEntry.EventId.Name),
            new PropertyItem("Category", logEntry.Category),
            new PropertyItem("Message", message),
            new PropertyItem("State", StateObject(logEntry.State)),
            new PropertyItem("Scopes", ScopesObject(scopeProvider)),
            new PropertyItem("Exception", ExceptionObject(logEntry.Exception))
        };

        foreach (var item in list)
            item.WriteTo(writer, indent: 0, tryJoin: false);

        writer.WriteLine();
    }

    private IItem StateObject(object? state)
    {
        // ignore message template without arguments
        if (state is not IReadOnlyCollection<KeyValuePair<string, object>> {Count: > 1} stateProperties)
            return NullItem.Instance;

        var properties = stateProperties.Select(item =>
        {
            var name = item.Key == "{OriginalFormat}" ? "MessageTemplate" : item.Key;
            return new PropertyItem(name, Item.Create(item.Value));
        });

        return Item.Create(properties);
    }

    private IItem ScopesObject(IExternalScopeProvider? scopeProvider)
    {
        if (!formatterOptions.IncludeScopes || scopeProvider == null)
            return NullItem.Instance;

        var items = new List<IItem>();
        scopeProvider.ForEachScope((scope, list) =>
        {
            if (scope != null) list.Add(Item.Create(scope));
        }, items);

        return Item.Create(items);
    }
    
    private IItem ExceptionObject(Exception? exception)
    {
        if (exception == null)
            return NullItem.Instance;

        var stackTrace = exception.StackTrace?.Split(Environment.NewLine).Select(x => new ValueItem(x));

        return new ObjectItem(
            new("Type", Item.Create(exception.GetType().FullName!)),
            new("Message", Item.Create(exception.Message)),
            new("StackTrace", Item.Create(stackTrace)),
            new("InnerException", ExceptionObject(exception.InnerException)));
    }
}
