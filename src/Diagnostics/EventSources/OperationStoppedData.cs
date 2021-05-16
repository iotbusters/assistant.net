using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Assistant.Net.Diagnostics.EventSources
{
    /// <summary>
    ///     Stopped operation event payload.
    /// </summary>
    [EventData]
    internal struct OperationStoppedData
    {
        /// <summary>
        ///     Operation correlation ID.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        ///     Operation duration.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        ///     Operation result message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Operation result status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Operation metadata.
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }
    }
}