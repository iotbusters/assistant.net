using System;

namespace Assistant.Net.Messaging.Models
{
    /// <summary>
    ///     Message handling related properties.
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
        ///     Correlation id under which it was initially requested.
        /// </summary>
        public string CorrelationId { get; private set; }

        /// <summary>
        ///     A date when it was initially requested.
        /// </summary>
        public DateTimeOffset Created { get; private set; }

        /// <summary>
        ///     A date when it was finally complete.
        /// </summary>
        public DateTimeOffset? Updated { get; private set; }

        /// <summary>
        ///     Creates an initial object.
        /// </summary>
        public static RecordProperties OnCreated(string correlationId, DateTimeOffset created) => new(correlationId, created, updated: null);

        /// <summary>
        ///     Creates new object based on current with additional property: updated.
        /// </summary>
        public RecordProperties OnUpdated(DateTimeOffset updated) => new(CorrelationId, Created, updated);
    }
}
