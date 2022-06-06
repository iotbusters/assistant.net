# assistant.net.messaging.web.client

Remote WEB oriented message handling provider implementation for messaging client which delegates actual handling
to an API hosted by [server](../Messaging.Web.Server/README.md).

## Usage

```csharp
using var provider = new ServiceCollection()
    .AddMessagingClient(b => b
        .UseWeb(b => b // configure HttpClient
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost/messages"))
            .AddHttpMessageHandler<CustomHttpHandler>())
        .AddWeb<SomeMessage>());

var client = provider.GetRequiredService<IMessagingClient>();
var response = await client.Request(new SomeMessage());

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
internal class CustomHttpHandler : DelegatingHandler { ... }
```
