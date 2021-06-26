# Assistant.NET

The solution is planned to help with automation of your small-sized processes and flows using IoT devices.

Currently, it's in design and implement stage, so the repository contains mostly tools and infrastructure parts only. And existing releases cannot be assumed as stable and backward compatible.

Hopefully it will be useful for someone once main functional is ready.

Please join also this [small survey](https://forms.gle/eB3sN5Mw76WMpT6w5).

## Releases

The paragraph isn't ready yet.

## Packages

A family of packages serve Assistant.NET needs and being distributed at [nuget.org](https://nuget.org). Each of them has own responsibility and solves some specific aspect of the solution.

### assistant.net.core

It's some basic abstractions and implementations which are commonly used across the solution. E.g. a system clock, an system lifetime management and improved overrides of .net extensions.

```csharp
services
    .AddSystemClock()
    .AddSystemLifetime();

var now = provider.GetRequiredService<ISystemClock>().UtcNow;

var stoppingCancellationToken = provider.GetRequiredService<ISystemLifetime>().Stopping;
```

### assistant.net.storage

It's common storage abstraction and related basic implementations and tools.

Pay attention, using base types as storage value won't work as expected because of serialization. It will be improved as part of advanced serialization further.

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

In progress. Coming soon..

It's serialization abstraction with 'basic' and 'advanced' json implementations.

Further I can be extended with other formats, e.g. protobuf. The main idea is to be able flexibly replace serialization format further.

```csharp
services.AddSerializer(b => b
    .AddJsonConverter<ModelJsonConverter>()
    .AddJsonType<Model>()
    .AddJsonTypeAny()
    );

var storage = provider.GetRequiredService<ISerializer<Model>>();
```

### assistant.net.diagnostics

It's diagnostics management tools including tracking operations, correlations, event tracing etc.

```csharp
services.AddDiagnostics();

var operation = provider.GetRequiredService<IDiagnosticFactory>().Start("operation");
// do something
operation.Complete("the operation is complete");

var currentCorrelationId = provider.GetRequiredService<IDiagnosticContext>().CorrelationId;
```

### assistant.net.messaging

It's local (in-memory) message handling implementation which support simple extending mechanism and basic message (internal term `command`) intercepting out of box.

See also available extensions in `assistant.net.messaging.*` packages for more information.

```csharp
services.AddCommandClient(b =>
{
    b.AddLocal<SomeCommandHandler>();
    b.AddInterceptor<SomeCommandInterceptor>();
});
var client = provider.GetRequiredService<ICommandClient>();
var response = await client.Send(new SomeCommand())
```

### assistant.net.messaging.web

It implements shared tools required by packages with remote WEB oriented handling implementation. E.g. json serialization.

See `assistant.net.messaging.web.*` packages for more information.

### assistant.net.messaging.web.client

It's a client implementation to remote WEB oriented message handling server.

```csharp
services
    .AddCommandClient(b => b.AddRemote<SomeCommand>())
    .AddRemoteWebCommandClient(opt => opt.BaseAddress = "https://localhost");

var client = provider.GetRequiredService<ICommandClient>();
var response = await client.Send(new SomeCommand())
```

See [server](#assistantnetmessagingwebserver) configuration for remote handling.

### assistant.net.messaging.web.server

It's a remote WEB oriented message handling server implementation. The server exposes API and accepts remote requests for further processing.

```csharp
services.AddRemoteWebCommandHandler(b =>
{
    b.AddLocal<SomeCommandHandler>();
    b.AddInterceptor<SomeCommandInterceptor>();
}); // it reuses `.AddCommandClient(b => { })` behind the scenes so they are fully compatible.
```

See [client](#assistantnetmessagingwebclient) configuration and remote command handling request.
