# assistant.net.messaging.sqlite.client

The package defines [generic server](https://www.nuget.org/packages/assistant.net.messaging.generic.client/) extensions
to configure SQLite providers for accepting requests from a [client](https://www.nuget.org/packages/assistant.net.messaging.sqlite.client/).

## Usage

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
// configure server for accepting remote messages and their delegation to local SomeMessageHandler.
services.AddSqliteMessageHandling(b => b
    .UseSqlite(o => o.Connection("Data Source=Messaging;Mode=Memory;Cache=Shared"))
    .AddHandler<SomeMessageHandler>());

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
```
