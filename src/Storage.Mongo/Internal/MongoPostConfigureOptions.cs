using Assistant.Net.Options;
using Assistant.Net.Storage.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Storage.Internal;

internal class MongoPostConfigureOptions : IPostConfigureOptions<MongoOptions>
{
    private readonly ILogger<MongoPostConfigureOptions> logger;
    private readonly HashSet<string> created = new();

    public MongoPostConfigureOptions(ILogger<MongoPostConfigureOptions> logger) =>
        this.logger = logger;

    public void PostConfigure(string name, MongoOptions options)
    {
        if(created.Contains(name) || !options.EnsureDatabaseCreated)
            return;

        logger.LogInformation("Ensure database is created.");

        var client = new MongoClient(options.ConnectionString);
        var database = client.GetDatabase(options.DatabaseName);
        var storageCollection = database.GetCollection<MongoRecord>(MongoNames.StorageCollectionName);
        var historicalCollection = database.GetCollection<MongoVersionedRecord>(MongoNames.HistoricalStorageCollectionName);

        AddIndex(storageCollection, b => b.Ascending(x => x.Key.Id).Ascending(x => x.Key.ValueType));
        AddIndex(historicalCollection, b => b.Ascending(x => x.Key.Key).Ascending(x => x.Key.Version));

        logger.LogInformation("Database is created.");

        created.Add(name);
    }

    private void AddIndex<T>(IMongoCollection<T> collection, Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> configure)
    {
        try
        {
            var indexKeysDefinition = configure(Builders<T>.IndexKeys);
            collection.Indexes.CreateOne(
                new CreateIndexModel<T>(indexKeysDefinition),
                new CreateOneIndexOptions());
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, $"Failed to add index to {collection.CollectionNamespace.CollectionName} collection.");
        }
    }
}
