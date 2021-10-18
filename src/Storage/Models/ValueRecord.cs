namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     Internal value representation object.
    /// </summary>
    /// <param name="Type">Value type name.</param>
    /// <param name="Content">Binary value content.</param>
    /// <param name="Audit">Value content auditing details.</param>
    public record ValueRecord(string Type, byte[] Content, Audit Audit);
}
