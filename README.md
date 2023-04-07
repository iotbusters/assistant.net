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

- [assistant.net.core](src/Core/README.md)
- [assistant.net.diagnostics](src/Diagnostics/README.md)
- [assistant.net.logging](src/Logging/README.md)
- [assistant.net.serialization](src/Serialization/README.md)
  - [assistant.net.serialization.json](src/Serialization.Json/README.md)
  - [assistant.net.serialization.proto](src/Serialization.Proto/README.md)
- [assistant.net.storage](src/Storage/README.md)
  - [assistant.net.storage.mongo](src/Storage.Mongo/README.md)
  - [assistant.net.storage.sqlite](src/Storage.Sqlite/README.md)
- [assistant.net.messaging](src/Messaging/README.md)
  - [assistant.net.messaging.mongo.client](src/Messaging.Mongo.Client/README.md)
  - [assistant.net.messaging.mongo.server](src/Messaging.Mongo.Server/README.md)
  - [assistant.net.messaging.sqlite.client](src/Messaging.Sqlite.Client/README.md)
  - [assistant.net.messaging.sqlite.server](src/Messaging.Sqlite.Server/README.md)
  - [assistant.net.messaging.web.client](src/Messaging.Web.Client/README.md)
  - [assistant.net.messaging.web.server](src/Messaging.Web.Server/README.md)
