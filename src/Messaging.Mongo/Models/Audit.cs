using System;
using System.Collections.Generic;

namespace Assistant.Net.Messaging.Models
{
    /// <summary>
    ///     Message handling related properties.
    /// </summary>
    public class Audit
    {
        /// <summary/>
        public Audit(IDictionary<string, object> details) =>
            Details = details;

        /// <summary/>
        public Audit() =>
            Details = new Dictionary<string, object>();

        /// <summary>
        ///     Message request related correlation id.
        /// </summary>
        public string? CorrelationId
        {
            get => Get<string?>("correlationId");
            set => Set("correlationId", value!);
        }

        /// <summary>
        ///     User requested message.
        /// </summary>
        public string? User
        {
            get => Get<string?>("user");
            set => Set("user", value!);
        }

        /// <summary>
        ///     The date when a message was requested.
        /// </summary>
        public DateTimeOffset? Requested
        {
            get => Get<DateTimeOffset?>("requested");
            set => Set("requested", value!);
        }

        /// <summary>
        ///     The date when a message was requested.
        /// </summary>
        public DateTimeOffset? Completed
        {
            get => Get<DateTimeOffset?>("completed");
            set => Set("completed", value!);
        }

        /// <summary>
        ///     All auditing details.
        /// </summary>
        public IDictionary<string, object> Details { get; set; }

        private void Set(string propertyName, object propertyValue) => Details[propertyName] = propertyValue;

        private T? Get<T>(string name) => Details.TryGetValue(name, out var value) ? (T?)value : default;
    }
}
