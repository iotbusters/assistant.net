# assistant.net.mongo.healthcheck

MongoDB options based health check implementation to monitor remote database readiness to accept requests.

## Usage

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
services
    // MongoOptions configuration
    .AddStorage(b => b
        .UseMongo(o => o.Connection("mongodb://127.0.0.1:27017").Database("Storage"))
        .AddMongo<SomeModel>()
    .AddHealthChecks()
    // Configured MongoOptions health checking
    .AddMongo();
```
