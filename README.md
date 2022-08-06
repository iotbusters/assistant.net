# Assistant.NET

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

Abstractions, basic implementations, useful tools and extensions which are commonly used across the solution.

See [assistant.net.core](src/Core/README.md) for details.
See also provider configuration extensions

- [assistant.net.mongo](src/Core.Mongo/README.md)
- [assistant.net.mongo](src/Core.Mongo.HealthCheck/README.md)
- [assistant.net.sqlite](src/Core.Sqlite/README.md)
- [assistant.net.sqlite](src/Core.Sqlite.HealthCheck/README.md).

### assistant.net.diagnostics

Diagnostics management tools including tracking operations, correlations, event tracing etc.

See [assistant.net.diagnostics](src/Diagnostics/README.md) for details.

### assistant.net.serialization.json

Generic serialization mechanism with flexible type/format configuration.
The only implementation for JSON for now. Further it will be extended with other formats, e.g. protobuf.

See [assistant.net.serialization.json](src/Serialization.Json/README.md) for details.

### assistant.net.storage

Basic storage implementation which is based on abstract storage providers including
local (in-memory) provider implementations out of box.

See [assistant.net.storage](src/Storage/README.md) for details.
See also available providers

- [assistant.net.storage.mongo](../Storage.Mongo/README.md)
- [assistant.net.storage.sqlite](../Storage.Sqlite/README.md)

### assistant.net.storage.mongo

MongoDB based storage provider implementation of [Storage](#assistantnetstorage).

See [assistant.net.storage.mongo](src/Storage.Mongo/README.md) for details.

### assistant.net.storage.sqlite

SQLite based storage provider implementation of [Storage](#assistantnetstorage).

See [assistant.net.storage.sqlite](src/Storage.Sqlite/README.md) for details.

### assistant.net.messaging

Basic message requesting/handling implementation which is based on abstract handling provider
with local (in-memory) provider implementation, message intercepting mechanism out of box.

See [assistant.net.messaging](src/Messaging/README.md) for details.

See also available providers

- [assistant.net.messaging.mongo.client](../Messaging.Mongo.Client/README.md)
- [assistant.net.messaging.mongo.server](../Messaging.Mongo.Server/README.md)
- [assistant.net.messaging.sqlite.client](../Messaging.Sqlite.Client/README.md)
- [assistant.net.messaging.sqlite.server](../Messaging.Sqlite.Server/README.md)
- [assistant.net.messaging.web.client](../Messaging.Web.Client/README.md)
- [assistant.net.messaging.web.server](../Messaging.Web.Server/README.md).

#### assistant.net.messaging.web.client

Remote WEB oriented message handling provider implementation for messaging client which delegates actual handling
to an API hosted by [server](#assistantnetmessagingwebserver).

See [assistant.net.messaging.web.client](src/Messaging.Web.Client/README.md) for details.
See also [assistant.net.messaging.web.server](src/Messaging.Web.Server/README.md).

#### assistant.net.messaging.web.server

Remote WEB oriented message handling server implementation which exposes API and accepts remote requests for further processing.
A [client](#assistantnetmessagingwebclient) can request message handling remotely by calling respective API.

See [assistant.net.messaging.web.server](src/Messaging.Web.Server/README.md) for details.
See also [assistant.net.messaging.web.client](src/Messaging.Web.Client/README.md).

#### assistant.net.messaging.mongo.client

Remote MongoDB based message handling provider implementation for messaging client which delegates actual handling
to a hosted [server](#assistantnetmessagingmongoserver).

See [assistant.net.messaging.mongo.client](src/Messaging.Mongo.Client/README.md) for details.
See also [assistant.net.messaging.mongo.server](src/Messaging.Mongo.Server/README.md).

#### assistant.net.messaging.mongo.server

Remote MongoDB based message handling server implementation which listen to the database and accepts remote messages
for further processing. [Client](#assistantnetmessagingmongoclient) can request message handling remotely
by storing requested message to the database.

See [assistant.net.messaging.mongo.server](src/Messaging.Mongo.Server/README.md) for details.
See also [assistant.net.messaging.mongo.client](src/Messaging.Mongo.Client/README.md).

#### assistant.net.messaging.sqlite.client

Remote SQLite based message handling provider implementation for messaging client which delegates actual handling
to a hosted [server](#assistantnetmessagingsqliteserver).

See [assistant.net.messaging.sqlite.client](src/Messaging.Sqlite.Client/README.md) for details.
See also [assistant.net.messaging.sqlite.server](src/Messaging.Sqlite.Server/README.md).

#### assistant.net.messaging.sqlite.server

Remote SQLite based message handling server implementation which listen to the database and accepts remote messages
for further processing. [Client](#assistantnetmessagingsqliteclient) can request message handling remotely
by storing requested message to the database.

See [assistant.net.messaging.sqlite.server](src/Messaging.Sqlite.Server/README.md) for details.
See also [assistant.net.messaging.sqlite.client](src/Messaging.Sqlite.Client/README.md).
