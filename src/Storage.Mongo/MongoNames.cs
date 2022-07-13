namespace Assistant.Net.Storage;

/// <summary>
///     MongoDB related resource names for data storing.
/// </summary>
public sealed class MongoNames
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
    ///     Historical/Partitioned storage collection name.
    /// </summary>
    public const string HistoricalStorageCollectionName = "HistoricalRecords";
}
