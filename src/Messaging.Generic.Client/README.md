# assistant.net.messaging.generic.client

Message handling client implementation which stores requested message to a storage delegating actual handling to
a hosted [server](https://www.nuget.org/packages/assistant.net.messaging.generic.server/) and it starts polling
a storage for a response.

> **Note**
> Storage based messaging is called `generic` as cross-provider implementation. Also it helps to avoid
> confusion between more clear `storage` and the same named package.

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
