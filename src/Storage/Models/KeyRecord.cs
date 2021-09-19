using System;

namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     Internal key representation object.
    /// </summary>
    public class KeyRecord
    {
        /// <summary/>
        public KeyRecord(string id, string type, byte[] content, Audit? audit = null)
        {
            Id = id;
            Type = type;
            Content = content;
            Audit = audit;
        }

        /// <summary>
        ///     Unique identifier across specific <see cref="Type" /> keys.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Binary key content.
        /// </summary>
        public byte[] Content { get; }

        /// <summary>
        ///     Key content auditing details.
        /// </summary>
        public Audit? Audit { get; }

        /// <summary>
        ///     Key type name.
        /// </summary>
        public string Type { get; }

        /// <inheritdoc cref="Equals(object?)"/>
        public bool Equals(KeyRecord key) => Type.Equals(key.Type) && Id.Equals(key.Id);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is KeyRecord key && Equals(key);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Type, Id);
    }
}
