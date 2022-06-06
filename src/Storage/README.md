# assistant.net.storage

Basic storage implementation which is based on abstract storage providers including
local (in-memory) provider implementations out of box.

## Usage

```csharp
using var provider = new ServiceCollection()
    .AddStorage(b => b
        .AddLocal<SomeModel>()
        .AddLocalAny()
        .AddLocalHistorical<SomeModel>()
        .AddLocalHistoricalAny()
        .AddLocalPartitioned<SomeModel>()
        .AddLocalPartitionedAny())
    .BuildStorageProvider();

var regularStorage = provider.GetRequiredService<IStorage<Key, SomeModel>>();
var partitionedStorage = provider.GetRequiredService<IHistoricalStorage<Key, SomeModel>>();
var partitionedStorage = provider.GetRequiredService<IPartitionedStorage<Key, SomeModel>>();
```

## Available providers

- [assistant.net.storage.mongo](https://www.nuget.org/packages/assistant.net.storage.mongo/)
- [assistant.net.storage.sqlite](https://www.nuget.org/packages/assistant.net.storage.sqlite/)
