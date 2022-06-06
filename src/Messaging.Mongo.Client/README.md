# assistant.net.messaging.mongo.client

Remote MongoDB based message handling provider implementation for messaging client which delegates actual handling
to a hosted [server](https://www.nuget.org/packages/assistant.net.messaging.mongo.server/).

## Usage

```csharp
using var provider = new ServiceCollection()
    .AddMessagingClient(b => b
        .UseMongo(o => o.Connection("mongodb://127.0.0.1:27017").Database("Messaging"))
        .AddMongo<SomeMessage>());

var client = provider.GetRequiredService<IMessagingClient>();
var response = await client.Request(new SomeMessage());

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
internal class CustomHttpHandler : DelegatingHandler { ... }
```
