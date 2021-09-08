using System;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     Internal value representation object.
    /// </summary>
    public class ValueRecord
    {
        /// <summary/>
        public ValueRecord(byte[] content, long version, DateTimeOffset created, DateTimeOffset? updated)
        {
            Content = content;
            Version = version;
            Created = created;
            Updated = updated;
        }

        /// <summary>
        ///     Binary value content.
        /// </summary>
        public byte[] Content { get; }

        /// <summary>
        ///     Value content version.
        /// </summary>
        public long Version { get; }

        /// <summary>
        ///     Value creating date.
        /// </summary>
        public DateTimeOffset Created { get; }

        /// <summary>
        ///     Value updating date.
        /// </summary>
        public DateTimeOffset? Updated { get; }
    }
}