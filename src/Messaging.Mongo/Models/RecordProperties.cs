using System;

namespace Assistant.Net.Messaging.Models
{
    /// <summary>
    ///     
    /// </summary>
    public class RecordProperties
    {
        /// <summary/>
        public RecordProperties(string correlationId, DateTimeOffset created, DateTimeOffset? updated)
        {
            CorrelationId = correlationId;
            Created = created;
            Updated = updated;
        }

        /// <summary>
        ///     
        /// </summary>
        public string CorrelationId { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public DateTimeOffset Created { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public DateTimeOffset? Updated { get; private set; }

        /// <summary>
        ///     
        /// </summary>
        public static RecordProperties OnCreated(string correlationId, DateTimeOffset created) => new(correlationId, created, updated: null);

        /// <summary>
        ///     
        /// </summary>
        public RecordProperties OnUpdated(DateTimeOffset updated) => new(CorrelationId, Created, updated);
    }
}
