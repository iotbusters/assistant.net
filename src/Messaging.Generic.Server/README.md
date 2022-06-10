# assistant.net.messaging.generic.server

Message handling server implementation which polls a storage accepting registered messages (others are skipped)
for further processing. On the other side [Client](https://www.nuget.org/packages/assistant.net.messaging.generic.client/)
requests message handling by storing a message to the same storage.

Current `generic` implementation is configured fully for a single provider approach to be able to choose quickly
a specific provider (calling `Use{Provider}()`, e.g. `UseMongo()`).

> **Note**
> Storage based messaging is called `generic` as cross-provider implementation. Also it helps to avoid
> confusion between more clear `storage` and the same named package.

## Hosting

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
services.AddGenericMessageHandling(b => b
    .UseMongo(o => o // configure specific (MongoDB) provider connection
        .Connection("mongodb://127.0.0.1:27017")
        .Database("Messaging"))
    .AddHandler<SomeMessageHandler>());

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
internal class SomeMessageHandler : IMessageHandler<SomeMessage> { ... }
```
