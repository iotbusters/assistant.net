namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     No message response representation.
/// </summary>
public sealed record Nothing
{
    private Nothing() { }

    /// <summary>
    ///     Instance of no message response.
    /// </summary>
    public static Nothing Instance => new();
}
