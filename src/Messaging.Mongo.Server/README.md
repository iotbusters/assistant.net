# assistant.net.messaging.mongo.server

The package defines [generic server](https://www.nuget.org/packages/assistant.net.messaging.generic.client/) extensions
to configure MongoDB providers for accepting requests from a [client](https://www.nuget.org/packages/assistant.net.messaging.mongo.client/).

## Usage

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
// configure server for accepting remote messages and their delegation to local SomeMessageHandler.
services.AddWebMessageHandling(b => b
    .UseSqlite(o => o.Connection("mongodb://127.0.0.1:27017").Database("Messaging"))
    .AddHandler<SomeMessageHandler>());

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
```
