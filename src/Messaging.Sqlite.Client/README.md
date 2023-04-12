# assistant.net.messaging.sqlite.client

A SQLite extension of [generic messaging client](https://www.nuget.org/packages/assistant.net.messaging.generic.client/)
communicating to a [server](https://www.nuget.org/packages/assistant.net.messaging.sqlite.server/).

## Configuration

1. [Configure generic messaging client](https://github.com/iotbusters/assistant.net/tree/master/src/Messaging.Generic.Client/README.md#configuration)
2. Configure SQLite storage provider

```csharp
services.ConfigureMessagingClient(b => b.UseSqlite(o => ...));
// or
services.ConfigureStorage(b => b.UseSqlite(o => ...)); // or other provider
```
