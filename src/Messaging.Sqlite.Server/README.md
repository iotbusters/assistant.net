# assistant.net.messaging.sqlite.server

Remote SQLite based message handling server implementation which listen to the database and accepts remote requests
for further processing. [Client](../Messaging.Sqlite.Client/README.md) can request message handling remotely
by storing a message to the database.

## Hosting

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
// configure server for accepting remote messages and their delegation to local SomeMessageHandler.
services.AddSqliteMessageHandling(b => b
    .UseSqlite(o => o.Connection("Data Source=Messaging;Mode=Memory;Cache=Shared"))
    .AddHandler<SomeMessageHandler>());

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
internal class SomeMessageHandler : IMessageHandler<SomeMessage> { ... }
```
