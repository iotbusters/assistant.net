# assistant.net.messaging.mongo.client

A MongoDB extension of [generic messaging client](https://www.nuget.org/packages/assistant.net.messaging.generic.client/)
communicating to a [server](https://www.nuget.org/packages/assistant.net.messaging.mongo.server/).

## Configuration

1. [Configure generic messaging client](https://github.com/iotbusters/assistant.net/tree/master/src/Messaging.Generic.Client/README.md#configuration)
2. Configure MongoDB storage provider

```csharp
services.ConfigureMessagingClient(b => b.UseMongo(o => ...));
// or
services.ConfigureStorage(b => b.UseMongo(o => ...)); // or other provider
```
