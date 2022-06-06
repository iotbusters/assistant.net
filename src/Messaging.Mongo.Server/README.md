# assistant.net.messaging.mongo.server

Remote MongoDB based message handling server implementation which listen to the database and accepts remote requests
for further processing. [Client](../Messaging.Mongo.Client/README.md) can request message handling remotely
by storing requested message to the database.

## Hosting

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
// configure server for accepting remote messages and their delegation to local SomeMessageHandler.
services.AddWebMessageHandling(b => b
    .UseSqlite(o => o.Connection("mongodb://127.0.0.1:27017").Database("Messaging"))
    .AddHandler<SomeMessageHandler>());

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
internal class SomeMessageHandler : IMessageHandler<SomeMessage> { ... }
```
