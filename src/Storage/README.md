# assistant.net.storage

Basic key-value based storage implementation designed to be easily extended and flexibly configured.
It gives an opportunity to isolate an application from specific data store provider.

## Default configuration

To add storage one of methods should be called:

```csharp
var services = new ServiceCollection()
    .AddStorage(b => ...) // adds dependencies and initial configuration
    .ConfigureStorage(b => ...); // appends existing configuration (no dependencies are added)
```

where a specific provider should be selected for the storing data

```csharp
services.ConfigureStorage(b => b.UseLocal()); // mongo or other
```

and key-value types allowed for storing

```csharp
services.ConfigureStorage(b => b
    .Add<SomeKey, SomeModel>() // by registering specific pairs
    .AllowAnyType()); // or allowing any type
```

## Configuration extension

Storage configuration may be big and complicated so applying it again and again can be annoying and error-prone.
But message configuration can help to avoid that

```csharp
services.ConfigureStorage(b => b
    .AddConfiguration<CustomStorageConfiguration>() // by a type parameter
    .AddConfiguration(new CustomStorageConfiguration())); // or an instance

public class CustomStorageConfiguration : IStorageConfiguration { ... }
```

## Named configuration

Storage implementation is based on
[named options](https://github.com/iotbusters/assistant.net/blob/master/Core/README.md#named-options)
so you can have multiple named storages with different configurations.

```csharp
var provider = new ServiceCollection()
    .AddStorage()
    .ConfigureStorage("name-1", o => o.UseLocal().Add<int, ModelOne>())
    .ConfigureStorage("name-2", o => o.UseMongo().Add<string, ModelTwo>())
    .BuildServiceProvider();

using var scope1 = provider.CreateScopeWithNamedOptionContext("name-1");
using var scope2 = provider.CreateScopeWithNamedOptionContext("name-2");

var storage1 = scope1.ServiceProvider.GetRequiredService<IStorage<int, ModelOne>>();
var storage2 = scope2.ServiceProvider.GetRequiredService<IStorage<string, ModelTwo>>();
```

## Storage resolving

Once storage was properly configured a specific storages can be resolved

```csharp
var storage = provider.GetRequiredService<IStorage<Key, SomeModel>>();
var historicalStorage = provider.GetRequiredService<IHistoricalStorage<Key, SomeModel>>();
var partitionedStorage = provider.GetRequiredService<IPartitionedStorage<Key, SomeModel>>();
```

or specific storages extended with operations like removing values, getting value details or stored keys

```csharp
var regularAdminStorage = provider.GetRequiredService<IAdminStorage<Key, SomeModel>>();
var historicalAdminStorage = provider.GetRequiredService<IHistoricalAdminStorage<Key, SomeModel>>();
var partitionedAdminStorage = provider.GetRequiredService<IPartitionedAdminStorage<Key, SomeModel>>();
```

Pay attention, if specific storage isn't properly configured, resolving it will throw an exception.

## Available providers

- Local provider (in-memory, available out of box),
- [MongoDb](https://www.nuget.org/packages/assistant.net.storage.mongo/),
- [SQLite](https://www.nuget.org/packages/assistant.net.storage.sqlite/).
