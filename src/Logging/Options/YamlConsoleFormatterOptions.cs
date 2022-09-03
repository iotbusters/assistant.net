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
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="state"></param>
    /// <exception cref="ArgumentException"></exception>
    public YamlConsoleFormatterOptions AddState(string name, object state)
    {
        if (state is Delegate and not Func<object> and not Func<IServiceProvider, object>)
            throw new ArgumentException("Delegate cannot be used as static state.", nameof(state));
        States[name] = state;
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="stateFactory"></param>
    public YamlConsoleFormatterOptions AddState(string name, Func<object> stateFactory) =>
        AddState(name, (object)stateFactory);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="stateFactory"></param>
    public YamlConsoleFormatterOptions AddState(string name, Func<IServiceProvider, object> stateFactory) =>
        AddState(name, (object)stateFactory);
}
