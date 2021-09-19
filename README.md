# Assistant.NET Common

The list of common packages responsible for solving specific aspects of the Assistant.NET solution.

Unfortunately existing solutions on a market didn't cover all needs or overcomplicated.
Beyond that, they weren't flexible enough to support new requirements further,
so the decision was to design and implement own set of tools.
Which although can be used as standalone packages too.

Currently, it's in design and implementation stage, so the repository contains mostly tools
and infrastructure parts only.
Existing releases cannot be assumed as stable and backward compatible too.
Pay attention during package upgrade!

Hopefully, it will be useful for someone once main functional is ready.

Please join this [quick survey](https://forms.gle/eB3sN5Mw76WMpT6w5).

## Releases

- [Assistant.NET Release 0.1.76](https://github.com/iotbusters/assistant.net/releases/tag/0.1.76)
  - introduced none backward compatible change to remote messaging
  - local storage refactoring
  - added mongo storage provider

- [Assistant.NET Release 0.1.72](https://github.com/iotbusters/assistant.net/releases/tag/0.1.72)
  - refactored `Serialization.Json` to support polimorphic and generic value types
  - refactored key-value storing mechanism in `Storage`
  - fixed caching interceptor issues related to serialization in `Storage`
- [Assistant.NET Release 0.1.71](https://github.com/iotbusters/assistant.net/releases/tag/0.1.71)
  - introduced none backward compatible change to serialization and storage related packages
- [Assistant.NET Release 0.1.70](https://github.com/iotbusters/assistant.net/releases/tag/0.1.70)
  - introduced none backward compatible change to messaging related packages
    - added cancellation token to messaging async operations
  - obsoleted task mapping extensions
- [Assistant.NET Release 0.1.69](https://github.com/iotbusters/assistant.net/releases/tag/0.1.69)
  - fixed local storage lifetime issue
- [Assistant.NET Release 0.1.68](https://github.com/iotbusters/assistant.net/releases/tag/0.1.68)
  - introduced none backward compatible change to messaging related packages
    - `command` related files were renamed to `message` to avoid ambiguity with patterns
- [Assistant.NET Release 0.1.65](https://github.com/iotbusters/assistant.net/releases/tag/0.1.65)
  - Bug fixes
  - Unused files deleted
  - Code docs updated
- [Assistant.NET Release 0.1.59](https://github.com/iotbusters/assistant.net/releases/tag/0.1.59)
  - Updated proxy generation packages
- [Release 0.1.42](https://github.com/iotbusters/assistant.net/releases/tag/0.1.42)
  - Added `Serialization.Json`
  - Refactoring and bug fixes
- [Release 0.1.40](https://github.com/iotbusters/assistant.net/releases/tag/0.1.40)
  - Added partitioned `Storage`
  - Refactoring and bug fixes

## Packages

A family of standalone packages serve Assistant.NET needs and being [freely](license) distributed
at [nuget.org](https://nuget.org). Each of them has own responsibility and solves some specific aspect of the solution.

### assistant.net.core

Basic abstractions and implementations which are commonly used across the solution.
E.g. a system clock, an system lifetime management and improved overrides of .net extensions.

```csharp
services
    .AddSystemClock()
    .AddSystemLifetime();

var now = provider.GetRequiredService<ISystemClock>().UtcNow;

var stoppingCancellationToken = provider.GetRequiredService<ISystemLifetime>().Stopping;
```

### assistant.net.storage

Common storage abstraction and related basic implementations and tools.

Pay attention, using base types as storage value won't work as expected because of serialization.
It will be improved as part of advanced serialization further.

See also available extensions in `assistant.net.storage.*` packages for more information.

```csharp
services.AddStorage(b => b
    .AddLocal<Model1>()
    .AddLocalAny()
    .AddLocalPartitioned<Model2>()
    );

var storage = provider.GetRequiredService<IStorage<Key, Model>>();
var partitionedStorage = provider.GetRequiredService<IPartitionedStorage<Key, Model>>();
```

### assistant.net.storage.mongo

MongoDB based storage extension of [Storage](#assistantnetstorage).

```csharp
services
    .AddStorage(b => b
        .UseMongo(o => o.ConnectionString = "mongodb://localhost")
        .AddMongo<Model1>()
        .AddMongoAny()
        .AddMongoPartitioned<Model2>()
        );

var storage = provider.GetRequiredService<IStorage<Key, Model>>();
var partitionedStorage = provider.GetRequiredService<IPartitionedStorage<Key, Model>>();
```

### assistant.net.serialization.json

Simple serialization abstraction with json implementation for now. Further it can be extended with other formats, e.g. protobuf.
The main idea is to be able flexibly choose serialization format for each type.

```csharp
services.AddSerializer(b => b
    .AddJsonConverter<ModelJsonConverter>()
    .AddJsonType<Model>()
    .AddJsonTypeAny()
    );

var typedSerializer = provider.GetRequiredService<ISerializer<Model>>();

var factory = provider.GetRequiredService<ISerializerFactory>();
var objectSerializer = factory.Create(typeof(Model));
```

### assistant.net.diagnostics

Diagnostics management tools including tracking operations, correlations, event tracing etc.

```csharp
services.AddDiagnostics();

var operation = provider.GetRequiredService<IDiagnosticFactory>().Start("operation");
// do something
operation.Complete("the operation is complete");

var currentScopeCorrelationId = provider.GetRequiredService<IDiagnosticContext>().CorrelationId;
```

### assistant.net.messaging

Local (in-memory) message handling implementation which support simple extending mechanism
and basic message intercepting out of box.

See also available extensions in `assistant.net.messaging.*` packages for more information.

```csharp
services.AddMessagingClient(b => b
    .AddLocal<SomeMessageHandler>()
    .AddInterceptor<SomeMessageInterceptor>()
    );

var client = provider.GetRequiredService<IMessagingClient>();
var response = await client.Send(new SomeMessage())
```

#### assistant.net.messaging.web

Shared tools required by packages related to remote WEB oriented message handling.
E.g. json serializer configuration.

See `assistant.net.messaging.web.*` packages for more information.

#### assistant.net.messaging.web.client

Remote WEB oriented message handling client implementation for [server](#assistantnetmessagingwebserver) API.

```csharp
services.AddMessagingClient(b => b
    .UseWebHandler(b => b
        .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost/messages"))
        .AddHttpMessageHandler<CustomDelegatingHandler>())
    .AddWeb<SomeMessage>()
    );

var client = provider.GetRequiredService<IMessagingClient>();
var response = await client.Send(new SomeMessage())
```

See [server](#assistantnetmessagingwebserver) configuration for remote handling.

#### assistant.net.messaging.web.server

Remote WEB oriented message handling server implementation. The server exposes API and accepts remote requests for further processing.

```csharp
services.AddRemoteWebMessageHandler(b => b
    .AddLocal<SomeMessageHandler>();
    .AddInterceptor<SomeMessageInterceptor>()
    ); // it reuses `.AddMessagingClient()` behind the scenes so they are fully compatible.
```

See [client](#assistantnetmessagingwebclient) configuration and remote message handling request.
