using System;

namespace Assistant.Net.Abstractions;

/// <summary>
///     An abstraction over logging scope object configuration.
/// </summary>
public interface ILoggerScopeBuilder : IDisposable
{
    /// <summary>
    ///     Adds a property to existing scope object.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="value">Property value.</param>
    ILoggerScopeBuilder AddPropertyScope(string name, object? value);

    /// <summary>
    ///     Adds a property to existing scope object.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="valueFactory">Property value factory.</param>
    ILoggerScopeBuilder AddPropertyScope(string name, Func<object?> valueFactory);

    /// <summary>
    ///     Adds a property to existing scope object.
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="valueFactory">Property value factory.</param>
    ILoggerScopeBuilder AddPropertyScope(string name, Func<IServiceProvider, object?> valueFactory);
}
