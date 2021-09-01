using System.Diagnostics.Tracing;

namespace Assistant.Net.Diagnostics.EventSources
{
    /// <summary>
    ///     Event source metadata item.
    /// </summary>
    [EventData]
    public class ItemData
    {
        /// <summary>
        ///     Metadata value.
        /// </summary>
        public string Value { get; set; } = null!;
    }
}