using Assistant.Net.Internal;
using Assistant.Net.Options;
using Microsoft.Extensions.Logging;
using System;
using static Microsoft.Extensions.Logging.ActivityTrackingOptions;

namespace Assistant.Net;

/// <summary>
///     Logging builder configuring extensions.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    ///     Add a console log formatter named 'yaml' to the factory with default properties.
    /// </summary>
    public static ILoggingBuilder AddYamlConsole(this ILoggingBuilder builder) => builder.AddYamlConsole(null!);

    /// <summary>
    ///     Add a console log formatter named 'yaml' to the factory with default properties.
    /// </summary>
    public static ILoggingBuilder AddYamlConsole(this ILoggingBuilder builder, Action<YamlConsoleFormatterOptions>? configure)
    {
        builder.Services.AddSystemClock();
        return builder
            .AddConsole(o => o.FormatterName = ConsoleFormatterNames.Yaml)
            .AddConsoleFormatter<YamlConsoleFormatter, YamlConsoleFormatterOptions>(o =>
            {
                o.IncludeScopes = true;
                o.TimestampFormat = "yyyy-MM-dd hh:mm:ss.fff";
                o.UseUtcTimestamp = true;

                configure?.Invoke(o);
            })
            .Configure(o => o.ActivityTrackingOptions = Tags);
    }

    /// <summary>
    ///     Adds a property to global scope object.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="name">Property name.</param>
    /// <param name="state">Property value.</param>
    public static ILoggingBuilder AddPropertyScope(this ILoggingBuilder builder, string name, object state)
    {
        builder.Services.Configure<YamlConsoleFormatterOptions>(o => o.AddState(name, state));
        return builder;
    }

    /// <summary>
    ///     Adds a property to global scope object.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="name">Property name.</param>
    /// <param name="stateFactory">Property value factory.</param>
    public static ILoggingBuilder AddPropertyScope(this ILoggingBuilder builder, string name, Func<object> stateFactory)
    {
        builder.Services.Configure<YamlConsoleFormatterOptions>(o => o.AddState(name, stateFactory));
        return builder;
    }

    /// <summary>
    ///     Adds a property to global scope object.
    /// </summary>
    /// <param name="builder"/>
    /// <param name="name">Property name.</param>
    /// <param name="stateFactory">Property value factory.</param>
    public static ILoggingBuilder AddPropertyScope(this ILoggingBuilder builder, string name, Func<IServiceProvider, object> stateFactory)
    {
        builder.Services.Configure<YamlConsoleFormatterOptions>(o => o.AddState(name, stateFactory));
        return builder;
    }
}
