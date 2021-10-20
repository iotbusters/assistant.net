using System;
using System.Collections.Generic;

namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     Associated value auditing details.
    /// </summary>
    public class Audit
    {
        /// <summary/>
        public Audit(IDictionary<string, object> details, long version)
        {
            Details = details;
            Version = version;
        }

        /// <summary/>
        public Audit(string? correlationId, string? user)
        {
            Details = new Dictionary<string, object>();
            CorrelationId = correlationId;
            User = user;
        }

        /// <summary>
        ///     Value change related correlation id.
        /// </summary>
        public string? CorrelationId
        {
            get => Get<string?>("correlationId");
            set => Set("correlationId", value!);
        }

        /// <summary>
        ///     User created value.
        /// </summary>
        public string? User
        {
            get => Get<string?>("user");
            set => Set("user", value!);
        }

        /// <summary>
        ///     Value state version.
        /// </summary>
        public long Version { get; }

        /// <summary>
        ///     The date when value was created.
        /// </summary>
        public DateTimeOffset? Created
        {
            get => Get<DateTimeOffset?>("created");
            set => Set("created", value!);
        }

        /// <summary>
        ///     All auditing details.
        /// </summary>
        public IDictionary<string, object> Details { get; set; }

        private void Set(string propertyName, object propertyValue) => Details[propertyName] = propertyValue;

        private T? Get<T>(string name) => Details.TryGetValue(name, out var value) ? (T?)value : default;
    }
}
