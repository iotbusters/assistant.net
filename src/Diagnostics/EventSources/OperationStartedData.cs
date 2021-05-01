using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Assistant.Net.Diagnostics.EventSources
{
    [EventData]
    internal struct OperationStartedData
    {
        public IDictionary<string, string> Metadata { get; set; }
    }
}