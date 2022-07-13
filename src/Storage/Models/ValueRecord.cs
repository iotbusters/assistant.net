namespace Assistant.Net.Storage.Models;

/// <summary>
///     Internal value representation object.
/// </summary>
/// <param name="Content">Binary value content.</param>
/// <param name="Audit">Value content auditing details.</param>
public record ValueRecord(byte[] Content, Audit Audit);
