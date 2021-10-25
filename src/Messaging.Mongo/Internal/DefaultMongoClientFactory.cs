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
        private readonly IOptions<MongoOptions> options;

        public DefaultMongoClientFactory(IOptions<MongoOptions> options) =>
            this.options = options;

        /// <inheritdoc/>
        public IMongoClient Create() => new MongoClient(options.Value.ConnectionString);
    }
}
