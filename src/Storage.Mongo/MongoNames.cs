namespace Assistant.Net.Storage
{
    /// <summary>
    ///     MongoDB related resource names for data storing.
    /// </summary>
    public class MongoNames
    {
        /// <summary>
        ///     Database name.
        /// </summary>
        public const string DatabaseName = "Storage";

        /// <summary>
        ///     Storage collection name.
        /// </summary>
        public const string StorageCollectionName = "Records";

        /// <summary>
        ///     Partitioned storage key collection name.
        /// </summary>
        public const string PartitionStorageKeyCollectionName = "PartitionKeys";

        /// <summary>
        ///     Partitioned storage key/value relation collection name.
        /// </summary>
        public const string PartitionStorageKeyValueCollectionName = "PartitionIndexes";

        /// <summary>
        ///     Partitioned storage value collection name.
        /// </summary>
        public const string PartitionStorageValueCollectionName = "PartitionRecords";
    }
}
