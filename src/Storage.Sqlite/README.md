# assistant.net.storage.sqlite

SQLite based storage provider implementation of [Storage](https://www.nuget.org/packages/assistant.net.storage/).

## Usage

```csharp
using var provider = new ServiceCollection()
    .AddStorage(b => b
        .UseSqlite(o => o.Connection("Data Source=Messaging;Mode=Memory;Cache=Shared"))
        .AddSqlite<SomeModel>()
        .AddSqliteAny()
        .AddSqliteHistorical<SomeModel>()
        .AddSqliteHistoricalAny()
        .AddSqlitePartitioned<SomeModel>()
        .AddSqlitePartitionedAny())
    .AddHealthChecks().AddSqlite().Services
    .BuildStorageProvider();

var options = provider.GetRequiredService<INamedOptions<SqliteOptions>>().Value;
var regularStorage = provider.GetRequiredService<IStorage<Key, SomeModel>>();
var partitionedStorage = provider.GetRequiredService<IHistoricalStorage<Key, SomeModel>>();
var partitionedStorage = provider.GetRequiredService<IPartitionedStorage<Key, SomeModel>>();
```