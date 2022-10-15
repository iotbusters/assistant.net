using Assistant.Net.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Assistant.Net.Internal;

/// <summary>
/// Produces instances of <see cref="ILogger"/> classes based on the given providers.
/// </summary>
/// <remarks>
///     See original <a href="https://github.com/aspnet/Logging/blob/master/src/Microsoft.Extensions.Logging/LoggerFactory.cs">source code</a>
/// </remarks>
internal sealed class DefaultLoggerFactory : ILoggerFactory
{
    private readonly IOptionsMonitor<LoggerFilterOptions> options;
    private readonly Dictionary<string, AggregatedLogger> cachedLoggers;
    private readonly ObservableCollection<ProviderRegistration> providerRegistrations;
    private readonly object sync = new();

    private bool isDisposed;

    /// <summary/>
    public DefaultLoggerFactory(IOptionsMonitor<LoggerFilterOptions> options, IEnumerable<ILoggerProvider> providers)
    {
        this.options = options;
        this.cachedLoggers = new(StringComparer.Ordinal);

        var registrations = providers.Select(x => new ProviderRegistration(x, isOwned: false));
        this.providerRegistrations = new(registrations);
    }

    /// <summary>
    /// Adds the given provider to those used in creating <see cref="ILogger"/> instances.
    /// </summary>
    /// <param name="provider">The <see cref="ILoggerProvider"/> to add.</param>
    void ILoggerFactory.AddProvider(ILoggerProvider provider)
    {
        if (isDisposed)
            throw new ObjectDisposedException(GetType().FullName);

        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        lock (sync)
            providerRegistrations.Add(new ProviderRegistration(provider, isOwned: true));
    }

    ILogger ILoggerFactory.CreateLogger(string categoryName)
    {
        if (isDisposed)
            throw new ObjectDisposedException(GetType().FullName);

        lock (sync)
        {
            if (cachedLoggers.TryGetValue(categoryName, out var logger))
                return logger;

            var definitions = providerRegistrations.Select(x => new LoggerDefinition(options, x.Provider, categoryName));
            var observable = new ObservableCollection<LoggerDefinition>(definitions);
            providerRegistrations.CollectionChanged += (_, args) =>
            {
                if (args.NewItems == null)
                    return;

                var registration = args.NewItems.Cast<ProviderRegistration>().Single();
                var definition = new LoggerDefinition(options, registration.Provider, categoryName);
                observable.Add(definition);
            };
            logger = new AggregatedLogger(observable);
            cachedLoggers[categoryName] = logger;

            return new DefaultLogger(logger);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (sync)
        {
            if (isDisposed)
                return;

            isDisposed = true;

            var registrations = providerRegistrations.Where(registration => registration.IsOwned);
            foreach (var registration in registrations)
                try
                {
                    registration.Provider.Dispose();
                }
                catch
                {
                    // suppress
                }
        }
    }
}

internal sealed class ProviderRegistration
{
    public ProviderRegistration(ILoggerProvider provider, bool isOwned)
    {
        Provider = provider;
        IsOwned = isOwned;
    }

    public ILoggerProvider Provider { get; }
    public bool IsOwned { get; }
}

internal sealed class AggregatedLogger : ILogger, IDisposable
{
    private readonly IExternalScopeProvider? scopeProvider;
    private readonly ObservableCollection<LoggerDefinition> definitions;
    private IDictionary<LogLevel, LoggerDefinition[]> enableMap = null!;

    public AggregatedLogger(ObservableCollection<LoggerDefinition> definitions)
    {
        this.definitions = definitions;
        this.scopeProvider = null;
        definitions.CollectionChanged += (_, _) => RefreshMap();
        RefreshMap();
    }

    private AggregatedLogger(AggregatedLogger logger)
    {
        this.scopeProvider = new LoggerExternalScopeProvider();

        var newDefinitions = logger.definitions.Select(x => x.Clone());
        this.definitions = new ObservableCollection<LoggerDefinition>(newDefinitions);
        logger.definitions.CollectionChanged += OnDefinitionsChanged;

        this.enableMap = logger.enableMap;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        List<Exception>? exceptions = null;
        foreach (var definition in enableMap[logLevel])
            try
            {
                definition.Instance.Log(logLevel, eventId, state, exception, formatter);
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>();
                exceptions.Add(ex);
            }

        if (exceptions != null)
            ThrowLoggingError(exceptions);
    }

    public bool IsEnabled(LogLevel logLevel) =>
        enableMap[logLevel].Any();

    public IDisposable BeginScope<TState>(TState state) =>
        scopeProvider?.Push(state) ?? throw new NotSupportedException("Scope isn't supported.");

    public AggregatedLogger AsScoped() => new(this);

    private void RefreshMap() =>
        enableMap = Enum.GetValues<LogLevel>().ToDictionary(
            lvl => lvl,
            lvl => definitions.Where(x => x.IsEnabled(lvl)).ToArray());

    public void Dispose()
    {
        logger.definitions.CollectionChanged -= OnDefinitionsChanged;

        foreach (var definition in definitions)
            definition.Dispose();
    }

    private void OnDefinitionsChanged(object? _, NotifyCollectionChangedEventArgs args)
    {
        if (args.NewItems == null) return;

        var definition = args.NewItems.Cast<LoggerDefinition>().Single();
        this.definitions.Add(definition.Clone());

        this.enableMap = logger.enableMap;
    }

    private static void ThrowLoggingError(IEnumerable<Exception> exceptions) =>
        throw new AggregateException("An error occurred while writing to logger(s).", exceptions);
}

internal sealed class DefaultLogger : ILogger
{
    private readonly object sync = new();

    private AggregatedLogger logger;
    private bool isScoped;

    public DefaultLogger(AggregatedLogger logger) =>
        this.logger = logger;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        logger.Log(logLevel, eventId, state, exception, formatter);

    public bool IsEnabled(LogLevel logLevel) =>
        logger.IsEnabled(logLevel);

    public IDisposable BeginScope<TState>(TState state)
    {
        lock (sync)
            if (!isScoped)
            {
                isScoped = true;
                logger = logger.AsScoped();
            }

        return logger.BeginScope(state);
    }
}

internal sealed class LoggerDefinition : IDisposable
{
    private static readonly Func<string, string, LogLevel, bool> alwaysEnabledFilter = delegate { return true; };

    private readonly object sync = new();

    private readonly IOptionsMonitor<LoggerFilterOptions> optionsMonitor;
    private readonly ILoggerProvider provider;
    private readonly string categoryName;
    private readonly Lazy<ILogger> loggerFactory;

    private readonly IDisposable disposable;

    private LogLevel minLevel = LogLevel.None;
    private Func<string, string, LogLevel, bool> filter = alwaysEnabledFilter;
    private bool isDisposed;

    public LoggerDefinition(
        IOptionsMonitor<LoggerFilterOptions> optionsMonitor,
        ILoggerProvider provider,
        string categoryName)
    {
        this.optionsMonitor = optionsMonitor;
        this.provider = provider;
        this.categoryName = categoryName;
        this.loggerFactory = new(() => provider.CreateLogger(categoryName));

        this.disposable = optionsMonitor.OnChange(Refresh);
        Refresh(optionsMonitor.CurrentValue);
    }

    public ILogger Instance
    {
        get
        {
            if(isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            return loggerFactory.Value;
        }
    }

    public bool IsEnabled(LogLevel level)
    {
        if (isDisposed)
            throw new ObjectDisposedException(GetType().FullName);

        return level >= minLevel && filter(GetType().FullName!, categoryName, level) || Instance.IsEnabled(level);
    }

    private void Refresh(LoggerFilterOptions options)
    {
        lock (sync)
        {
            var rule = options.GetRule(GetType(), categoryName);
            filter = rule?.Filter ?? alwaysEnabledFilter;
            minLevel = rule?.LogLevel ?? options.MinLevel;
        }
    }

    public void Dispose()
    {
        lock (sync)
        {
            if (isDisposed)
                return;

            isDisposed = true;

            disposable.Dispose();

            if(loggerFactory.IsValueCreated && loggerFactory.Value is IDisposable logger)
                logger.Dispose();
        }
    }

    public LoggerDefinition Clone() =>
        new LoggerDefinition(optionsMonitor, provider, categoryName);
}

internal interface IScopedLogger
{
    void SetScopeProvider(IExternalScopeProvider scopeProvider);
}

/// <summary>
/// 
/// </summary>
/// <remarks>
///     See original <a href="https://github.com/aspnet/Logging/blob/master/src/Microsoft.Extensions.Logging/LoggerRuleSelector.cs">source code</a>
/// </remarks>
internal static class LoggerRuleSelector
{
    public static LoggerFilterRule? GetRule(this LoggerFilterOptions options, Type providerType, string category)
    {
        // Filter rule selection:
        // 1. Select rules for current logger type, if there is none, select ones without logger type specified
        // 2. Select rules with longest matching categories
        // 3. If there nothing matched by category take all rules without category
        // 3. If there is only one rule use it's level and filter
        // 4. If there are multiple rules use last
        // 5. If there are no applicable rules use global minimal level

        var providerAlias = GetAlias(providerType);
        LoggerFilterRule? current = null;
        foreach (var rule in options.Rules)
            if (IsBetter(rule, current, providerType.FullName, category)
                || (!string.IsNullOrEmpty(providerAlias) && IsBetter(rule, current, providerAlias, category)))
                return rule;

        return null;
    }

    private static bool IsBetter(LoggerFilterRule rule, LoggerFilterRule? current, string? logger, string category)
    {
        // Skip rules with inapplicable type or category
        if (rule.ProviderName != null && rule.ProviderName != logger)
            return false;

        var categoryName = rule.CategoryName;
        if (categoryName != null)
        {
            const char wildcardChar = '*';

            var wildcardIndex = categoryName.IndexOf(wildcardChar);
            if (wildcardIndex != -1 && categoryName.IndexOf(wildcardChar, wildcardIndex + 1) != -1)
                throw new InvalidOperationException("More than one wildcard.");

            ReadOnlySpan<char> prefix, suffix;
            if (wildcardIndex == -1)
            {
                prefix = categoryName.AsSpan();
                suffix = default;
            }
            else
            {
                prefix = categoryName.AsSpan(0, wildcardIndex);
                suffix = categoryName.AsSpan(wildcardIndex + 1);
            }

            if (!category.AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                !category.AsSpan().EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (current?.ProviderName != null)
        {
            if (rule.ProviderName == null)
            {
                return false;
            }
        }
        else
        {
            // We want to skip category check when going from no provider to having provider
            if (rule.ProviderName != null)
            {
                return true;
            }
        }

        if (current?.CategoryName != null)
        {
            if (rule.CategoryName == null)
            {
                return false;
            }

            if (current.CategoryName.Length > rule.CategoryName.Length)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// See original <a href="https://github.com/aspnet/Logging/blob/master/src/Microsoft.Extensions.Logging/ProviderAliasUtilities.cs">source code.</a>
    /// </remarks>
    internal static string? GetAlias(Type providerType)
    {
        const string AliasAttibuteTypeFullName = "Microsoft.Extensions.Logging.ProviderAliasAttribute";
        var attributes = CustomAttributeData.GetCustomAttributes(providerType);

        for (var i = 0; i < attributes.Count; i++)
        {
            var attributeData = attributes[i];
            if (attributeData.AttributeType.FullName == AliasAttibuteTypeFullName &&
                attributeData.ConstructorArguments.Count > 0)
            {
                CustomAttributeTypedArgument arg = attributeData.ConstructorArguments[0];

                Debug.Assert(arg.ArgumentType == typeof(string));

                return arg.Value?.ToString();
            }
        }

        return null;
    }
}

/// <summary>
///     A provider of <see cref="ConsoleLogger" /> instances.
/// </summary>
/// <remarks>
///     See original <a href="https://github.com/aspnet/Logging/blob/master/src/Microsoft.Extensions.Logging.Console/ConsoleLoggerProvider.cs">source code</a>
/// </remarks>
internal sealed class ScopedConsoleLoggerProvider : ILoggerProvider
{
    internal const int MaxQueuedMessages = 1024;

    private readonly BlockingCollection<LogRecord> logQueue;
    private readonly ConcurrentDictionary<string, ConsoleLogger> loggers;
    private readonly Thread messageProcessingThread;
    private readonly Func<string, ConsoleLogger> loggerFactory;
    private readonly IDisposable disposable;

    private LogLevel standardErrorThreshold;

    /// <summary/>
    public ScopedConsoleLoggerProvider(
        IOptionsMonitor<ConsoleLoggerOptions> options,
        IEnumerable<ConsoleFormatter> formatters)
    {
        this.logQueue = new(MaxQueuedMessages);

        this.loggers = new(StringComparer.OrdinalIgnoreCase);
        this.messageProcessingThread = StartMessageProcessing();

        loggerFactory = name => new(name, logQueue, options, formatters.ToDictionary(x => x.Name));
        this.disposable = options.OnChange(RefreshStandardErrorThreshold);
        RefreshStandardErrorThreshold(options.CurrentValue);
    }

    private void RefreshStandardErrorThreshold(ConsoleLoggerOptions options) =>
        this.standardErrorThreshold = options.LogToStandardErrorThreshold;

    /// <inheritdoc />
    public ILogger CreateLogger(string name)
    {
        var key = name ?? throw new ArgumentNullException(nameof(name));
        return loggers.GetOrAdd(key, loggerFactory);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        logQueue.CompleteAdding();

        try
        {
            messageProcessingThread.Join(1500); // with timeout in-case Console is locked by user input
        }
        catch (ThreadStateException) { }

        disposable.Dispose();
    }

    private Thread StartMessageProcessing()
    {
        var thread = new Thread(() =>
        {
            try
            {
                foreach (var record in logQueue.GetConsumingEnumerable())
                    WriteMessage(record);
            }
            catch
            {
                try
                {
                    logQueue.CompleteAdding();
                }
                catch
                {
                    // suppress
                }
            }
        })
        {
            IsBackground = true,
            Name = "Console logger queue processing"
        };
        thread.Start();

        return thread;
    }

    private void WriteMessage(LogRecord record)
    {
        var textWriter = record.LogLevel >= this.standardErrorThreshold ? Console.Error : Console.Out;
        textWriter.Write(record.Message);
    }
}


/// <remarks>
///     See original <a href="https://github.com/aspnet/Logging/blob/master/src/Microsoft.Extensions.Logging.Console/ConsoleLogger.cs">source code</a>
/// </remarks>
internal sealed class ConsoleLogger : IScopedLogger, ILogger, IDisposable
{
    [ThreadStatic]
    private static readonly StringWriter writer;

    private readonly string name;
    private readonly BlockingCollection<LogRecord> logQueue;
    private readonly Dictionary<string, ConsoleFormatter> formatters;
    private readonly IDisposable disposable;

    private IExternalScopeProvider? scopeProvider;
    private ConsoleFormatter formatter = null!;

    static ConsoleLogger() =>
        writer = new(new StringBuilder(ScopedConsoleLoggerProvider.MaxQueuedMessages));

    public ConsoleLogger(
        string name,
        BlockingCollection<LogRecord> logQueue,
        IOptionsMonitor<ConsoleLoggerOptions> options,
        Dictionary<string, ConsoleFormatter> formatters)
    {
        this.name = name;
        this.logQueue = logQueue;
        this.formatters = formatters;
        this.disposable = options.OnChange(RefreshFormatter);
        RefreshFormatter(options.CurrentValue);
    }

    private IExternalScopeProvider ScopeProvider => scopeProvider ??= new LoggerExternalScopeProvider();

    public void SetScopeProvider(IExternalScopeProvider provider) =>
        scopeProvider = provider ?? throw new ArgumentNullException(nameof(provider));

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> logFormatter)
    {
        if (!IsEnabled(logLevel))
            return;

        if (logFormatter == null)
            throw new ArgumentNullException(nameof(logFormatter));

        var logEntry = new LogEntry<TState>(logLevel, name, eventId, state, exception, logFormatter!);
        formatter.Write(in logEntry, ScopeProvider, writer);

        var sb = writer.GetStringBuilder();
        if (sb.Length == 0)
            return;

        var message = sb.ToString();
        sb.Clear();

        if (!logQueue.IsAddingCompleted)
            logQueue.TryAdd(new LogRecord(message, logLevel));
    }

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None;

    public IDisposable BeginScope<TState>(TState state) =>
        ScopeProvider.Push(state);

    void IDisposable.Dispose() =>
        disposable.Dispose();

    private void RefreshFormatter(ConsoleLoggerOptions consoleOptions)
    {
        // warning:
        // ReloadLoggerOptions can be called before the ctor completed,...
        // before registering all of the state used in this method need to be initialized
        var formatterName = consoleOptions.FormatterName;

        formatter = string.IsNullOrEmpty(formatterName) || !formatters.TryGetValue(formatterName, out var logFormatter)
            ? formatters[ConsoleFormatterNames.Yaml]
            : logFormatter;
    }
}

internal readonly struct LogRecord
{
    public LogRecord(string message, LogLevel logLevel)
    {
        Message = message;
        LogLevel = logLevel;
    }

    public readonly string Message;
    public readonly LogLevel LogLevel;
}
