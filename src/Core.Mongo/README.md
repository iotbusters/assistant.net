# assistant.net.mongo

MongoDB configuration extension methods.

## Configure options

```csharp
using var provider = new ServiceCollection()
    .Configure<MongoOptions>(o => o.Connection(connectionString).Database(databaseName))
    .ConfigureMongoOptions(configuration)
    .BuildServiceProvider();

var options = provider.GetRequiredService<IOptions<MongoOptions>>().Value;
```

## Configure builder

```csharp
var builder = new SomeBuilder().UseMongo(o => o.Connection(connectionString).Database(databaseName));

internal class SomeBuilder : IBuilder<SomeBuilder> { ... }
```
