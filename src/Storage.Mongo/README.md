# assistant.net.storage.mongo

MongoDB based storage provider implementation of [Storage](https://www.nuget.org/packages/assistant.net.storage/).

## Usage

```csharp
using var provider = new ServiceCollection()
    .AddStorage(b => b
        .UseMongo(o => o.Connection("mongodb://127.0.0.1:27017").Database("Storage"))
        .AddMongo<SomeModel>()
        .AddMongoAny()
        .AddMongoHistorical<SomeModel>()
        .AddMongoHistoricalAny()
        .AddMongoPartitioned<SomeModel>()
        .AddMongoPartitionedAny())
    .BuildStorageProvider();
    
var regularStorage = provider.GetRequiredService<IStorage<Key, SomeModel>>();
var partitionedStorage = provider.GetRequiredService<IHistoricalStorage<Key, SomeModel>>();
var partitionedStorage = provider.GetRequiredService<IPartitionedStorage<Key, SomeModel>>();
```