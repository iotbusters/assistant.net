using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Assistant.Net.Messaging.Internal
{
    /// <summary>
    ///     Default MongoDB client factory for messaging provider.
    /// </summary>
    internal class DefaultMongoClientFactory : IMongoClientFactory
    {
        private readonly IOptionsSnapshot<MongoOptions> options;

        public DefaultMongoClientFactory(IOptionsSnapshot<MongoOptions> options) =>
            this.options = options;

        public IMongoClient CreateClient(string name) => new MongoClient(options.Get(name).ConnectionString);

        public IMongoDatabase GetDatabase(string name) => CreateClient(name).GetDatabase(options.Get(name).DatabaseName);
    }
}
