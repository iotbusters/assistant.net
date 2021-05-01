using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Assistant.Net.Diagnostics.EventSources
{
    [EventData]
    internal struct OperationStoppedData
    {
        public TimeSpan Duration { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }
}