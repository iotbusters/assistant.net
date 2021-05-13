using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Assistant.Net.Diagnostics.EventSources
{
    /// <summary>
    ///     Started operation event payload.
    /// </summary>
    [EventData]
    internal struct OperationStartedData
    {
        /// <summary>
        ///     Operation metadata.
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }
    }
}