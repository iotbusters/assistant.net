# assistant.net.messaging.web.server

Remote WEB oriented message handling server implementation which exposes API and accepts remote requests for further processing.
[Client](../Messaging.Web.Client/README.md) can request message handling remotely by calling respective API.

## Hosting

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
// configure server for accepting remote messages and their delegation to local SomeMessageHandler.
services.AddWebMessageHandling(b => b.AddHandler<SomeMessageHandler>());

// Startup.cs: void Configure(IApplicationBuilder app)
app.UseRemoteWebMessageHandler();

internal class SomeMessage : IMessage<SomeResponse> { ... }
internal class SomeResponse { ... }
internal class SomeMessageHandler : IMessageHandler<SomeMessage> { ... }
```
