using System;

namespace Assistant.Net.Messaging.HealthChecks;

/// <summary>
///     System.Type extensions for health checks of generic message handling on server.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    ///     Gets named object logger name.
    /// </summary>
    /// <param name="type">Object type.</param>
    /// <param name="name">Object name.</param>
    public static string GetLoggerName(this Type type, string name) => $"{type}(\"{name}\")";
}
