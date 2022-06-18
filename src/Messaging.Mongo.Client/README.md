﻿# assistant.net.messaging.mongo.client

The package defines [generic client](https://www.nuget.org/packages/assistant.net.messaging.generic.client/) extensions
to configure MongoDB providers required to connect a [server](https://www.nuget.org/packages/assistant.net.messaging.mongo.server/)
from a client.

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