namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Message handling instance name representation.
/// </summary>
public static class InstanceName
{
    /// <summary>
    ///     Creates an instance name.
    /// </summary>
    /// <param name="applicationName">Current application name.</param>
    /// <param name="optionName">Associated configuration name.</param>
    public static string Create(string applicationName, string optionName) => $"{applicationName}-{optionName}";
}
