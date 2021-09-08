using System;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     Internal key representation object.
    /// </summary>
    public class KeyRecord
    {
        /// <summary/>
        public KeyRecord(string id, byte[] content, string type)
        {
            Id = id;
            Content = content;
            Type = type;
        }

        /// <summary>
        ///     Uniquely key identifier across specific <see cref="Type" /> keys.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     Binary key content.
        /// </summary>
        public byte[] Content { get; }

        /// <summary>
        ///     Key type name.
        /// </summary>
        public string Type { get; }

        public bool Equals(KeyRecord key) => Type.Equals(key.Type) && Id.Equals(key.Id);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is KeyRecord key && Equals(key);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Type, Id);
    }
}