using Assistant.Net.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
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
    public static ILoggingBuilder AddYamlConsole(this ILoggingBuilder builder)
    {
        builder.Services.AddSystemClock();
        return builder
            .AddJsonConsole()
            .AddConsole(o => o.FormatterName = ConsoleFormatterNames.Yaml)
            .AddConsoleFormatter<YamlConsoleFormatter, ConsoleFormatterOptions>(o =>
            {
                o.IncludeScopes = true;
                o.TimestampFormat = "yyyy-MM-dd hh:mm:ss.fff";
                o.UseUtcTimestamp = true;
            })
            .Configure(o => o.ActivityTrackingOptions = ParentId | Tags);
    }
}
