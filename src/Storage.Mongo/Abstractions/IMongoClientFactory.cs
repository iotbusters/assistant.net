using MongoDB.Driver;

namespace Assistant.Net.Storage.Abstractions
{
    /// <summary>
    ///     MongoDB client factory for storage provider.
    /// </summary>
    public interface IMongoClientFactory
    {
        /// <summary>
        ///     Creates MongoDB client instance.
        /// </summary>
        IMongoClient CreateClient();

        /// <summary>
        ///     Gets MongoDB client's database instance.
        /// </summary>
        IMongoDatabase GetDatabase();
    }
}
