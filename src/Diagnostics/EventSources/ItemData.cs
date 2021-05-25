using System.Diagnostics.Tracing;

namespace Assistant.Net.Diagnostics.EventSources
{
    [EventData]
    public class ItemData
    {
        public string Value { get; set; } = null!;
    }
}