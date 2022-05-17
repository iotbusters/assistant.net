using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Assistant.Net.Internal;

/// <summary>
///     Default MongoDB client factory for MongoDB based providers.
/// </summary>
internal class DefaultMongoClientFactory : IMongoClientFactory
{
    private readonly IOptionsMonitor<MongoOptions> options;

    public DefaultMongoClientFactory(IOptionsMonitor<MongoOptions> options) =>
        this.options = options;

    public IMongoClient CreateClient(string name) => new MongoClient(options.Get(name).ConnectionString);

    public IMongoDatabase GetDatabase(string name) => CreateClient(name).GetDatabase(options.Get(name).DatabaseName);
}
