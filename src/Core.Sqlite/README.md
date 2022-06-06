# assistant.net.mongo

SQLite configuration extension methods.

## Configure options

```csharp
using var provider = new ServiceCollection()
    .Configure<SqliteOptions>(o => o.Connection(connectionString))
    .ConfigureSqliteOptions(configuration)
    .BuildServiceProvider();

var options = provider.GetRequiredService<IOptions<SqliteOptions>>().Value;
```

## Configure builder

```csharp
var builder = new SomeBuilder().UseSqlite(o => o.Connection(connectionString));

internal class SomeBuilder : IBuilder<SomeBuilder> { ... }
```
