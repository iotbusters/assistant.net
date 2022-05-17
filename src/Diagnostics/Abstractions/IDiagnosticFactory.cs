namespace Assistant.Net.Diagnostics.Abstractions;

/// <summary>
///     
/// </summary>
public interface IDiagnosticFactory
{
    /// <summary>
    ///     Starts new operation within current operation context.
    /// </summary>
    IOperation Start(string name);
}