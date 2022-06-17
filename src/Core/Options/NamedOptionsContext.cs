namespace Assistant.Net.Options;

/// <summary>
///     Default named options context.
/// </summary>
/// <remarks>
///     It can be used by related scoped features as shared context.
/// </remarks>
public sealed class NamedOptionsContext
{
    /// <summary>
    ///     The name of options instance.
    /// </summary>
    public string Name { get; internal set; } = Microsoft.Extensions.Options.Options.DefaultName;
}
