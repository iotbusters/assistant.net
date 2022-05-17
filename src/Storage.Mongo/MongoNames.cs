namespace Assistant.Net.Storage;

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
    ///     Historical/Partitioned storage key collection name.
    /// </summary>
    public const string HistoricalStorageKeyCollectionName = "Keys";

    /// <summary>
    ///     Historical/Partitioned storage key/value relation collection name.
    /// </summary>
    public const string HistoricalStorageKeyValueCollectionName = "KeyValues";

    /// <summary>
    ///     Historical/Partitioned storage value collection name.
    /// </summary>
    public const string HistoricalStorageValueCollectionName = "Values";
}