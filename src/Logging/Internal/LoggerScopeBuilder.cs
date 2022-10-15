using Assistant.Net.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Internal;

/// <summary>
///     Logging scope builder implementation.
/// </summary>
internal sealed class LoggerScopeBuilder : ILoggerScopeBuilder
{
    private readonly Dictionary<string, object> properties = new();
    private readonly IDisposable scope;

    /// <summary/>
    public LoggerScopeBuilder(ILogger logger) =>
        this.scope = logger.BeginScope(properties);

    /// <inheritdoc/>
    public ILoggerScopeBuilder AddPropertyScope(string name, object? value)
    {
        var key = name ?? throw new ArgumentNullException(nameof(name));
        if (value != null)
            properties[key] = value;

        return this;
    }

    /// <inheritdoc/>
    public ILoggerScopeBuilder AddPropertyScope(string name, Func<object?> valueFactory) =>
        AddPropertyScope(name, (object)valueFactory);

    /// <inheritdoc/>
    public ILoggerScopeBuilder AddPropertyScope(string name, Func<IServiceProvider, object?> valueFactory) =>
        AddPropertyScope(name, (object)valueFactory);

    /// <inheritdoc />
    public void Dispose() =>
        scope.Dispose();
}
