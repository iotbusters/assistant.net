# assistant.net.mongo.healthcheck

SQLite options based health check implementation to monitor remote database readiness to accept requests.

## Usage

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
services
    // SqliteOptions configuration
    .AddStorage(b => b
        .UseSqlite(o => o.Connection("Data Source=Messaging;Mode=Memory;Cache=Shared"))
        .AddSqlite<SomeModel>()
    .AddHealthChecks()
    // Configured SqliteOptions health checking
    .AddSqlite();
```
