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

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

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
    .AddLocalHistorical<Model2>()
    .AddLocalHistoricalAny()
    .AddLocalPartitioned<Model2>()
    .AddLocalPartitionedAny()
    );

var storage = provider.GetRequiredService<IStorage<Key, Model>>();
var partitionedStorage = provider.GetRequiredService<IPartitionedStorage<Key, Model>>();
```

### assistant.net.storage.mongo

MongoDB based storage extension of [Storage](#assistantnetstorage).

```csharp
services
    .AddStorage(b => b
        .UseMongo(o => o.Connection("mongodb://127.0.0.1:27017").Database("Storage"))
        .AddMongo<Model1>()
        .AddMongoAny()
        .AddMongoHistorical<Model2>()
        .AddMongoHistoricalAny()
        .AddMongoPartitioned<Model3>()
        .AddMongoPartitionedAny()
        );

var storage = provider.GetRequiredService<IStorage<Key, Model>>();
var partitionedStorage = provider.GetRequiredService<IPartitionedStorage<Key, Model>>();
```

### assistant.net.storage.sqlite

SQLite based storage extension of [Storage](#assistantnetstorage).

```csharp
services
    .AddStorage(b => b
        .UseSqlite(o => o.Connection("Data Source=:memory:"))
        .AddSqlite<Model1>()
        .AddSqliteAny()
        .AddSqliteHistorical<Model2>()
        .AddSqliteHistoricalAny()
        .AddSqlitePartitioned<Model3>()
        .AddSqlitePartitionedAny()
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
    .AddHandler<SomeMessageHandler>()
    .AddInterceptor<SomeMessageInterceptor>()
    );

var client = provider.GetRequiredService<IMessagingClient>();
var response = await client.Send(new SomeMessage())
```

#### assistant.net.messaging.web.client

Remote WEB oriented message handling client implementation for [server](#assistantnetmessagingwebserver) API.

```csharp
services.AddMessagingClient(b => b
    .UseWeb(b => b
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
services
    .AddWebMessageHandling(b => b.AddHandler<SomeMessageHandler>()) // accepting remote messages and delegating to local message client
    .ConfigureMessagingClient(b => b.AddInterceptor<SomeMessageInterceptor>() // local messaging client on a server
    );
```

See [client](#assistantnetmessagingwebclient) configuration and remote message handling request.

#### assistant.net.messaging.mongo.client

Remote MongoDB based message handling client implementation. The client stores message requests into configured database.

```csharp
services.AddMessagingClient(b => b
    .UseMongo(o => o.Connection("mongodb://127.0.0.1:27017").Database("Messaging"))
    .AddMongo<SomeMessage>()
    );

var client = provider.GetRequiredService<IMessagingClient>();
var response = await client.Send(new SomeMessage())
```

See [server](#assistantnetmessagingmongoserver) configuration for remote handling stored requests.

#### assistant.net.messaging.mongo.server

Remote MongoDB based message handling server implementation. The server actively polls configured database for new requested messages to handle.

```csharp
services
    .AddMongoMessageHandling(b => b
        .UseSqlite(o => o.Connection("mongodb://127.0.0.1:27017").Database("Messaging"))
        .AddHandler<SomeMessageHandler>()) // accepting remote messages and delegating to local message client
    .ConfigureMessagingClient(b => b
        .AddInterceptor<SomeMessageInterceptor>() // local messaging client on a server
    );
```

See [client](#assistantnetmessagingmongoclient) configuration and remote message handling request.

#### assistant.net.messaging.sqlite.client

Remote SQLite based message handling client implementation. The client stores message requests into configured database.

```csharp
services.AddMessagingClient(b => b
    .UseSqlite(o => o.Connection("Data Source=:memory:"))
    .AddMongo<SomeMessage>()
    );

var client = provider.GetRequiredService<IMessagingClient>();
var response = await client.Send(new SomeMessage())
```

See [server](#assistantnetmessagingsqliteserver) configuration for remote handling stored requests.

#### assistant.net.messaging.sqlite.server

Remote SQLite based message handling server implementation. The server actively polls configured database for new requested messages to handle.

```csharp
services
    .AddSqliteMessageHandling(b => b
        .UseSqlite(o => o.Connection("Data Source=:memory:"))
        .AddHandler<SomeMessageHandler>()) // accepting remote messages and delegating to local message client
    .ConfigureMessagingClient(b => b.AddInterceptor<SomeMessageInterceptor>() // local messaging client on a server
    );
```

See [client](#assistantnetmessagingsqliteclient) configuration and remote message handling request.
