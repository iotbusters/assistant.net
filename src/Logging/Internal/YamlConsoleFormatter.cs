using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assistant.Net.Internal;

internal sealed class YamlConsoleFormatter : ConsoleFormatter, IDisposable
{
    public const string OriginalFormatName = "{OriginalFormat}";

    private YamlConsoleFormatterOptions formatterOptions;
    private readonly IDisposable disposable;
    private readonly IServiceProvider serviceProvider;
    private readonly ISystemClock clock;

    public YamlConsoleFormatter(
        IOptionsMonitor<YamlConsoleFormatterOptions> options,
        IServiceProvider serviceProvider,
        ISystemClock clock) : base(ConsoleFormatterNames.Yaml)
    {
        this.formatterOptions = options.CurrentValue;
        this.disposable = options.OnChange(o => formatterOptions = o);
        this.serviceProvider = serviceProvider;
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
            new PropertyItem("Timestamp", Value(dateTimeString)),
            new PropertyItem("LogLevel", Value(logEntry.LogLevel)),
            new PropertyItem("EventId", Value(logEntry.EventId.Id)),
            new PropertyItem("EventName", Value(logEntry.EventId.Name)),
            new PropertyItem("Category", Value(logEntry.Category)),
            new PropertyItem("Message", Value(message)),
            new PropertyItem("State", StateObject(logEntry.State)),
            new PropertyItem("Scopes", ScopeArray(scopeProvider)),
            new PropertyItem("Exception", ExceptionObject(logEntry.Exception))
        };

        foreach (var item in list)
            item.WriteTo(writer, indent: 0, tryJoin: false);

        writer.WriteLine();
    }

    private static IItem StateObject(object? state)
    {
        if (state is not IEnumerable<KeyValuePair<string, object>> pairs)
            throw new ArgumentException("Unexpected type.", nameof(state));

        var array = pairs.ToArray();
        if (array.Length is 0 || array.Length is 1 && array[0].Key == OriginalFormatName)
            return NullItem.Instance;

        var items = array
            .Select(x => x.Key == OriginalFormatName
                ? new KeyValuePair<string, object>("MessageTemplate", x.Value)
                : x)
            .Select(x =>
            {
                var value = x.Key.StartsWith("@") ? Item.CreateObject(x.Value) : Value(x.Value);
                return new PropertyItem(x.Key, value);
            });
        return Item.CreateObject(items);
    }

    private IItem ScopeArray(IExternalScopeProvider? scopeProvider)
    {
        if (!formatterOptions.IncludeScopes || scopeProvider == null)
            return NullItem.Instance;

        var globalStates = formatterOptions.States.Select(x => x.Value switch
        {
            Func<object> stateFactory => new KeyValuePair<string, object>(x.Key, stateFactory()),
            Func<IServiceProvider, object> stateFactory => new KeyValuePair<string, object>(x.Key, stateFactory(serviceProvider)),
            _ => x
        });
        var scopes = new List<object?> {globalStates};
        scopeProvider.ForEachScope((scope, list) => list.Add(scope), scopes);

        var items = scopes.Select(x =>
        {
            if (x is not IEnumerable<KeyValuePair<string, object>> e)
                return Value(x);

            var pairs = e.Select(y => y.Value switch
            {
                Func<object> stateFactory => new KeyValuePair<string, object>(y.Key, stateFactory()),
                Func<IServiceProvider, object> stateFactory => new KeyValuePair<string, object>(y.Key, stateFactory(serviceProvider)),
                _ => y
            });
            return StateObject(pairs);
        });

        return Item.CreateArray(items);
    }
    
    private IItem ExceptionObject(Exception? exception)
    {
        if (exception == null)
            return NullItem.Instance;

        return new ObjectItem(
            new("Type", Value(exception.GetType().FullName!)),
            new("Message", Value(exception.Message)),
            new("StackTrace", Value(exception.StackTrace)),
            new("InnerException", ExceptionObject(exception.InnerException)));
    }

    private static IItem Value(object? value) => Item.CreateValue(value);
}
