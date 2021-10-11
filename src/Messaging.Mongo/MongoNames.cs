namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     MongoDB related resource names for message handling.
    /// </summary>
    public static class MongoNames
    {
        /// <summary>
        ///     Database name.
        /// </summary>
        public const string DatabaseName = "Messaging";

        /// <summary>
        ///     Message request/response coordination related collection name.
        /// </summary>
        public const string MessageCollectionName = "Records";

    }
}
