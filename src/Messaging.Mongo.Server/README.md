# assistant.net.messaging.mongo.server

A MongoDB extension of [generic messaging server](https://www.nuget.org/packages/assistant.net.messaging.generic.server/)
communicating to a [generic messaging client](https://www.nuget.org/packages/assistant.net.messaging.mongo.client/).

## Configuration

1. [Configure generic messaging server](https://github.com/iotbusters/assistant.net/tree/master/src/Messaging.Generic.Server/README.md#configuration)
2. Configure MongoDB storage provider

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
services.ConfigureGenericMessageHandling(b => b.UseMongo(o => ...));
// or named messaging client
services.ConfigureMessagingClient(GenericOptionsNames.DefaultName, b => b.UseMongo(o => ...));
```
