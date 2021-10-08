using MongoDB.Bson.Serialization.Attributes;

namespace Assistant.Net.Storage.Models
{
    /// <summary>
    ///     MongoDB storage persisting record.
    /// </summary>
    public class MongoRecord
    {
        /// <summary>
        ///     Bson serialization only.
        /// </summary>
        public MongoRecord() { }

        /// <summary />
        public MongoRecord(
            string id,
            string keyType,
            byte[] keyContent,
            string valueType,
            byte[] valueContent,
            Audit audit)
        {
            Id = id;
            KeyType = keyType;
            KeyContent = keyContent;
            ValueType = valueType;
            ValueContent = valueContent;
            Audit = audit;
        }

        /// <summary>
        ///     Unique identifier.
        /// </summary>
        [BsonId]
        public string Id { get; set; } = default!;

        /// <summary>
        ///     Key type name.
        /// </summary>
        public string KeyType { get; set; } = default!;

        /// <summary>
        ///     Binary key content.
        /// </summary>
        public byte[] KeyContent { get; set; } = default!;

        /// <summary>
        ///     Value type name.
        /// </summary>
        public string ValueType { get; set; } = default!;

        /// <summary>
        ///     Binary value content.
        /// </summary>
        public byte[] ValueContent { get; set; } = default!;

        /// <summary>
        ///     Value content auditing details.
        /// </summary>
        public Audit Audit { get; set; } = default!;
    }
}
