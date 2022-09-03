using Assistant.Net.Abstractions;
using Assistant.Net.Internal;
using Microsoft.Extensions.Logging;
using System;

namespace Assistant.Net;

/// <summary>
///     Logger configuring extensions.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    ///     Begins logging property scope.
    /// </summary>
    public static ILoggerScopeBuilder BeginPropertyScope(this ILogger logger) =>
        new LoggerScopeBuilder(logger);

    /// <summary>
    ///     Begins logging property scope with a property.
    /// </summary>
    /// <param name="logger"/>
    /// <param name="name">Property name.</param>
    /// <param name="value">Property value.</param>
    public static ILoggerScopeBuilder BeginPropertyScope(this ILogger logger, string name, object? value) =>
        logger.BeginPropertyScope().AddPropertyScope(name, value);

    /// <summary>
    ///     Begins logging property scope with a property.
    /// </summary>
    /// <param name="logger"/>
    /// <param name="name">Property name.</param>
    /// <param name="valueFactory">Property value factory.</param>
    public static ILoggerScopeBuilder BeginPropertyScope(this ILogger logger, string name, Func<object?> valueFactory) =>
        logger.BeginPropertyScope().AddPropertyScope(name, valueFactory);

    /// <summary>
    ///     Begins logging property scope with a property.
    /// </summary>
    /// <param name="logger"/>
    /// <param name="name">Property name.</param>
    /// <param name="valueFactory">Property value factory.</param>
    public static ILoggerScopeBuilder BeginPropertyScope(this ILogger logger, string name, Func<IServiceProvider, object?> valueFactory) =>
        logger.BeginPropertyScope().AddPropertyScope(name, valueFactory);
}
