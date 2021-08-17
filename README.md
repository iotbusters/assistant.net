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
    .AddLocal<Model>()
    .AddLocalAny()
    .AddLocalPartitioned<Model>()
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
and basic message (internal term `command`) intercepting out of box.

See also available extensions in `assistant.net.messaging.*` packages for more information.

```csharp
services.AddCommandClient(b => b
    .AddLocal<SomeCommandHandler>()
    .AddInterceptor<SomeCommandInterceptor>()
    );

var client = provider.GetRequiredService<ICommandClient>();
var response = await client.Send(new SomeCommand())
```

#### assistant.net.messaging.web

Shared tools required by packages related to remote WEB oriented message handling.
E.g. json serializer configuration.

See `assistant.net.messaging.web.*` packages for more information.

#### assistant.net.messaging.web.client

Remote WEB oriented message handling client implementation for [server](#assistantnetmessagingwebserver) API.

```csharp
services
    .AddCommandClient(b => b.AddRemote<SomeCommand>())
    .AddRemoteWebCommandClient(opt => opt.BaseAddress = "https://localhost");

var client = provider.GetRequiredService<ICommandClient>();
var response = await client.Send(new SomeCommand())
```

See [server](#assistantnetmessagingwebserver) configuration for remote handling.

#### assistant.net.messaging.web.server

Remote WEB oriented message handling server implementation. The server exposes API and accepts remote requests for further processing.

```csharp
services.AddRemoteWebCommandHandler(b => b
    .AddLocal<SomeCommandHandler>();
    .AddInterceptor<SomeCommandInterceptor>()
    ); // it reuses `.AddCommandClient()` behind the scenes so they are fully compatible.
```

See [client](#assistantnetmessagingwebclient) configuration and remote command handling request.
