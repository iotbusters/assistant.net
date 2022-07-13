using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage;
using Assistant.Net.Storage.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

internal class ConfigureMongoHostedService : IHostedService
{
    private readonly ILogger<ConfigureMongoHostedService> logger;
    private readonly IServiceScopeFactory scopeFactory;

    public ConfigureMongoHostedService(ILogger<ConfigureMongoHostedService> logger, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken token)
    {
        await using var scope = scopeFactory.CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var storageCollection = database.GetCollection<MongoRecord>(MongoNames.StorageCollectionName);
        var historicalCollection = database.GetCollection<MongoVersionedRecord>(MongoNames.HistoricalStorageCollectionName);

        await AddIndex(storageCollection, b => b.Ascending(x => x.Key.Id).Ascending(x => x.Key.ValueType), token);
        await AddIndex(historicalCollection, b => b.Ascending(x => x.Key.Key).Ascending(x => x.Key.Version), token);
    }

    public Task StopAsync(CancellationToken token) => Task.CompletedTask;

    private async Task AddIndex<T>(
        IMongoCollection<T> collection,
        Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> configure,
        CancellationToken token)
    {
        try
        {
            var indexKeysDefinition = configure(Builders<T>.IndexKeys);
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<T>(indexKeysDefinition),
                new CreateOneIndexOptions(),
                token);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, $"Failed to add index to {collection.CollectionNamespace.CollectionName} collection.");
        }
    }
}
