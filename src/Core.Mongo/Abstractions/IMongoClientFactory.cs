using MongoDB.Driver;

namespace Assistant.Net.Abstractions
{
    /// <summary>
    ///     MongoDB client factory for storage provider.
    /// </summary>
    public interface IMongoClientFactory
    {
        /// <summary>
        ///     Creates MongoDB client instance.
        /// </summary>
        /// <param name="name">Client configuration name.</param>
        IMongoClient CreateClient(string name);

        /// <summary>
        ///     Gets MongoDB client's database instance.
        /// </summary>
        /// <param name="name">Client configuration name.</param>
        IMongoDatabase GetDatabase(string name);
    }
}
