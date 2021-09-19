using System;

namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     Associated data auditing details.
    /// </summary>
    /// <param name="Version">Data version.</param>
    /// <param name="Created">The date when data creation happened on.</param>
    /// <param name="Updated">The date when last data update happened on.</param>
    public record Audit(long Version, DateTimeOffset Created, DateTimeOffset? Updated = null)
    {
        /// <summary>
        ///     Creates an initial auditing version.
        /// </summary>
        public static Audit Initial(DateTimeOffset created) => new(1, created);

        /// <summary>
        ///     Creates an auditing copy with incremented version.
        /// </summary>
        public Audit IncrementVersion(DateTimeOffset updated) => new(Version + 1, Created, updated);
    }
}
