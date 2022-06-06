# assistant.net.messaging

Basic message requesting/handling implementation which is based on abstract handling provider
including local (in-memory) provider implementation, message intercepting mechanism out of box.

## Local message handling

```csharp
using var provider = new ServiceCollection()
    .AddMessagingClient(b => b
        .AddHandler<LocalMessageHandler>()
        .AddInterceptor<SomeMessageInterceptor>())
    .BuildServiceProvider();

var client = provider.GetRequiredService<IMessagingClient>();
var response = await client.Request(new SomeMessage());

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
internal class LocalMessageHandler : IMessageHandler<SomeMessage, SomeResponse> { ... }
internal class SomeMessageInterceptor : IMessageInterceptor<SomeMessage, SomeResponse> { ... }
```

## Available providers

- [assistant.net.messaging.mongo.client](https://www.nuget.org/packages/assistant.net.messaging.mongo.client/)
- [assistant.net.messaging.mongo.server](https://www.nuget.org/packages/assistant.net.messaging.mongo.server/)
- [assistant.net.messaging.sqlite.client](https://www.nuget.org/packages/assistant.net.messaging.sqlite.client/)
- [assistant.net.messaging.sqlite.server](https://www.nuget.org/packages/assistant.net.messaging.sqlite.server/)
- [assistant.net.messaging.web.client](https://www.nuget.org/packages/assistant.net.messaging.web.client/)
- [assistant.net.messaging.web.server](https://www.nuget.org/packages/assistant.net.messaging.web.server/)
