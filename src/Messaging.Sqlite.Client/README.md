# assistant.net.messaging.sqlite.client

Remote MongoDB based message handling provider implementation for messaging client which delegates actual handling
to a hosted [server](../Messaging.Sqlite.Server/README.md).

## Usage

```csharp
using var provider = new ServiceCollection()
    .AddMessagingClient(b => b
        .UseSqlite(o => o.Connection("Data Source=Messaging;Mode=Memory;Cache=Shared"))
        .AddSqlite<SomeMessage>());

var client = provider.GetRequiredService<IMessagingClient>();
var response = await client.Request(new SomeMessage());

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
internal class CustomHttpHandler : DelegatingHandler { ... }
```
