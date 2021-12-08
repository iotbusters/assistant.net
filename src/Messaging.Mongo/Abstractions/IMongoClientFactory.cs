using MongoDB.Driver;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     MongoDB client factory for messaging provider.
    /// </summary>
    public interface IMongoClientFactory
    {
        /// <summary>
        ///     Creates MongoDB client default instance.
        /// </summary>
        IMongoClient CreateClient() => CreateClient(Microsoft.Extensions.Options.Options.DefaultName);

        /// <summary>
        ///     Creates MongoDB client named instance.
        /// </summary>
        IMongoClient CreateClient(string name);

        /// <summary>
        ///     Gets MongoDB client's default database instance.
        /// </summary>
        IMongoDatabase GetDatabase() => GetDatabase(Microsoft.Extensions.Options.Options.DefaultName);

        /// <summary>
        ///     Gets MongoDB client's named database instance.
        /// </summary>
        IMongoDatabase GetDatabase(string name);
    }
}
