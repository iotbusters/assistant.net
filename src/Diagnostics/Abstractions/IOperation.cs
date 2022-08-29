namespace Assistant.Net.Diagnostics.Abstractions;

/// <summary>
///     Tracking operation abstraction.
/// </summary>
public interface IOperation
{
    /// <summary>
    ///     Associates additional data to current operation.
    /// </summary>
    /// <param name="name">Data name.</param>
    /// <param name="value">Data value.</param>
    IOperation AddData(string name, string value);

    /// <summary>
    ///     Successfully complete current operation.
    /// </summary>
    /// <param name="message">Operation success message. if needed.</param>
    void Complete(string? message = null);

    /// <summary>
    ///     Fail current operation.
    /// </summary>
    /// <param name="message">Operation failure message. if needed.</param>
    void Fail(string? message = null);
}
