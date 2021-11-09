using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Assistant.Net.Storage.Internal
{
    /// <summary>
    ///     Default MongoDB client factory for storage provider.
    /// </summary>
    internal class DefaultMongoClientFactory : IMongoClientFactory
    {
        private readonly IOptions<MongoOptions> options;

        public DefaultMongoClientFactory(IOptions<MongoOptions> options) =>
            this.options = options;

        public IMongoClient CreateClient() => new MongoClient(options.Value.ConnectionString);

        public IMongoDatabase GetDatabase() => CreateClient().GetDatabase(options.Value.DatabaseName);
    }
}
