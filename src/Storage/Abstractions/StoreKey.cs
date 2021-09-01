namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     Internal store key representation object.
    /// </summary>
    public sealed class StoreKey
    {
        /// <summary/>
        public StoreKey() { }

        /// <summary/>
        public StoreKey(string id, string type, byte[] keyValue)
        {
            Id = id;
            Type = type;
            KeyValue = keyValue;
        }

        /// <summary>
        ///     Key identifier. Unique in context of <see cref="Type" />.
        /// </summary>
        public string Id { get; init; } = null!;

        /// <summary>
        ///     Specific key type name.
        /// </summary>
        public string Type { get; init; } = null!;

        /// <summary>
        ///     Specific key serialized object.
        /// </summary>
        public byte[] KeyValue { get; init; } = null!;

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is StoreKey key && Type.Equals(key.Type) && Id.Equals(key.Id);

        /// <inheritdoc/>
        public override int GetHashCode() => Id.GetHashCode();
    }
}