using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Options;

/// <summary>
/// 
/// </summary>
public class YamlConsoleFormatterOptions : ConsoleFormatterOptions
{
    internal Dictionary<string, object> States { get; } = new();

    /// <summary>
    ///     Adds named value to global scope.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="state">Property value.</param>
    /// <exception cref="ArgumentException"></exception>
    public YamlConsoleFormatterOptions AddScope(string name, object? state)
    {
        if (state == null)
            return this;

        if (state is Delegate and not Func<object> and not Func<IServiceProvider, object>)
            throw new ArgumentException("Delegate cannot be used as static state.", nameof(state));

        States[name] = state;
        return this;
    }

    /// <summary>
    ///     Adds named value factory to global scope.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="stateFactory">Property value factory.</param>
    /// <exception cref="ArgumentException"></exception>
    public YamlConsoleFormatterOptions AddScope(string name, Func<object?> stateFactory) =>
        AddScope(name, (object)stateFactory);

    /// <summary>
    ///     Adds named value factory to global scope.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="stateFactory">Property value factory.</param>
    /// <exception cref="ArgumentException"></exception>
    public YamlConsoleFormatterOptions AddScope(string name, Func<IServiceProvider, object?> stateFactory) =>
        AddScope(name, (object)stateFactory);
}
