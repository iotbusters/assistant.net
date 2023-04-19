namespace Assistant.Net.Storage.HealthChecks;

/// <summary>
///     Generic handling server health check names.
/// </summary>
public static class HealthCheckNames
{
    private const string NamePrefix = "storage-";

    /// <summary>
    ///     Creates a storage health check name by its option name.
    /// </summary>
    public static string CreateName(string optionName) => string.Concat(NamePrefix, optionName);

    /// <summary>
    ///     Restores an option name of storage from its health check name.
    /// </summary>
    public static string? GetOptionName(string healthCheckName) =>
        healthCheckName.StartsWith(NamePrefix)
            ? healthCheckName[NamePrefix.Length..]
            : null;
}
