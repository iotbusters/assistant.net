# assistant.net.messaging.sqlite.client

The package defines [generic client](https://www.nuget.org/packages/assistant.net.messaging.generic.client/) extensions
to configure SQLite providers required to connect a [server](https://www.nuget.org/packages/assistant.net.messaging.sqlite.server/)
from a client.

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
```
