# assistant.net.messaging.generic.server

A [messaging](https://www.nuget.org/packages/assistant.net.messaging/) server extension which handles messages from
a [generic client](https://www.nuget.org/packages/assistant.net.messaging.generic.client/) communicating via the same storage.

## Configuration

1. Configure message handling on a hosted server

```csharp
// Startup.cs: void ConfigureServices(IServiceCollection services)
services
    .AddGenericMessageHandling(b => ...) // adds dependencies and initial configuration
    .ConfigureGenericMessageHandling(b => ...); // appends existing configuration (no dependencies are added)
```

2. Register message handlers

```csharp
services.ConfigureGenericMessageHandling(b => b.AddHandler<SomeMessageHandler>());
// or named messaging client
services.ConfigureMessageClient(b => b.AddHandler<SomeMessageHandler>());
```

> **Note**
> As configured message handler will be run in non-default
> [named options](https://github.com/iotbusters/assistant.net/blob/master/Core/README.md#named-options)
> so all its dependencies should be configured the same named context or create own appropriately named DI scope
> to get dependencies from another named context.

3. Configure a single message handler

```csharp
services.ConfigureGenericMessageHandling(b => b.UseMongo(o => ...)); // or other provider
// or
services.ConfigureMessageClient(b => b.UseMongo(o => ...)); // or other provider
// or
services.ConfigureStorage(b => b.UseMongo(o => ...)); // or other provider
```

4. [Configure messaging client](https://github.com/iotbusters/assistant.net/blob/master/Messaging/README.md#default-configuration)
for generic message handling additionally.

## Named configuration

Message handling implementation is based on
[named options](https://github.com/iotbusters/assistant.net/blob/master/Core/README.md#named-options)
so you can have multiple named message handling servers with different configurations.

```csharp
services
    .AddGenericMessageHandling(b => b.AddHandler<Handle1>()) // default name registration
    .AddGenericMessageHandling("name-1", b => b.AddHandler<Handle2>()) // named registration
    .AddGenericMessageHandling("name-2", b => b.AddHandler<Handle3>()); // named registration
```
