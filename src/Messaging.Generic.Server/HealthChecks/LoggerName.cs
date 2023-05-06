using System;

namespace Assistant.Net.Messaging.HealthChecks;

/// <summary>
///     TODO
/// </summary>
internal static class LoggerName
{
    /// <summary>
    ///     TODO
    /// </summary>
    /// <param name="type">TODO</param>
    /// <param name="name">TODO</param>
    public static string ToLoggerName(this Type type, string name) => $"{type}(\"{name}\")";
}
