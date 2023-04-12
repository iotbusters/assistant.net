# assistant.net.messaging.generic.client

A [messaging](https://www.nuget.org/packages/assistant.net.messaging/) client extension which delegates message handling to
a [generic server](https://www.nuget.org/packages/assistant.net.messaging.generic.server/) communicating via the same storage.

## Configuration

1. Configure a storage based single message handler and an accepting message type

```csharp
services.ConfigureMessagingClient(b => b
    .UseGenericSingleHandler() // requires a storage provider to be configured
    .AddSingle<SomeMessage>());
```

2. Configure a storage provider

```csharp
services.ConfigureMessagingClient(b => b.UseMongo(o => ...)); // or other provider
// or
services.ConfigureStorage(b => b.UseMongo(o => ...)); // or other provider
```
