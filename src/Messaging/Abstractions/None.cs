namespace Assistant.Net.Messaging.Abstractions;

/// <summary>
///     No message response representation.
/// </summary>
public sealed class None
{
    private None() { }

    /// <summary>
    ///     Instance of none.
    /// </summary>
    public static None Instance { get; } = new();
}
