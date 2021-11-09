using MongoDB.Driver;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     MongoDB client factory for messaging provider.
    /// </summary>
    public interface IMongoClientFactory
    {
        /// <summary>
        ///     Creates MongoDB client instance.
        /// </summary>
        IMongoClient CreateClient(string name);

        /// <summary>
        ///     Gets MongoDB client's database instance.
        /// </summary>
        IMongoDatabase GetDatabase(string name);
    }
}
