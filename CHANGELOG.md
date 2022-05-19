# Assistant.NET Changelog

All relevant changes to packages which were released or being prepared for releasing.

See also [keepachangelog.com](https://keepachangelog.com/en/1.0.0/).

## [0.2.106] - 2022-05-19

[Assistant.NET Release 0.2.107](https://github.com/iotbusters/assistant.net/releases/tag/0.2.107)

### 0.2.106 Fix

- configure options source extension method

## [0.2.106] - 2022-05-17

[Assistant.NET Release 0.2.106](https://github.com/iotbusters/assistant.net/releases/tag/0.2.106)

### 0.2.106 Added

- sqlite storage and messaging providers

### 0.2.106 Changed

- mongo messaging providers to use mongo storage instead of custom implementation

### 0.2.106 Fixed

- bugs in messaging client
  - interceptor invalid ordering
  - interceptor invalid options injecting
  - storage invalid options registering

### 0.2.106 Removed

- messaging web/mongo packagees

## [0.2.105] - 2022-05-05

[Assistant.NET Release 0.2.105](https://github.com/iotbusters/assistant.net/releases/tag/0.2.105)

### 0.2.105 Added

- options binding extensions

### 0.2.105 Changed

- refactored mongo storage providers

### 0.2.105 Fixed

- issues with options binding

## [0.2.103] - 2022-02-13

[Assistant.NET Release 0.2.103](https://github.com/iotbusters/assistant.net/releases/tag/0.2.103)

### 0.2.103 Changed

- refactored options dependency mechanism

### 0.2.103 Fixed

- issues with options

## [0.2.102] - 2021-12-20

[Assistant.NET Release 0.2.102](https://github.com/iotbusters/assistant.net/releases/tag/0.2.102)

### 0.2.102 Changed

- upgraded to .net version 6

## [0.2.101] - 2021-12-16

[Assistant.NET Release 0.2.101](https://github.com/iotbusters/assistant.net/releases/tag/0.2.101)

### 0.2.101 Changed

- reworked messaging client and handling configuraiton builders to hide unsupported options

## [0.1.100] - 2021-12-08

[Assistant.NET Release 0.1.100](https://github.com/iotbusters/assistant.net/releases/tag/0.1.100)

### 0.1.100 Changed

- replaced dedicated messaging server options with named messaging client options
- mongo options naming to be compatible with messaging client options naming

## [0.1.99] - 2021-12-05

[Assistant.NET Release 0.1.99](https://github.com/iotbusters/assistant.net/releases/tag/0.1.99)

### 0.1.99 Added

- core options source concept to reload options without configuration layer
- named messaging clients

### 0.1.99 Changed

- referenced messaging server options to messaging client options reload

## [0.1.98] - 2021-11-13

[Assistant.NET Release 0.1.98](https://github.com/iotbusters/assistant.net/releases/tag/0.1.98)

### 0.1.98 Added

- mongo and web message handlers removal and cleanup during configuration

## [0.1.97] - 2021-11-13

[Assistant.NET Release 0.1.97](https://github.com/iotbusters/assistant.net/releases/tag/0.1.97)

### 0.1.97 Added

- local message handlers removal and cleanup during configuration

### 0.1.97 Changed

- message handlers and interceptors registration

### 0.1.97 Removed

- message handling providers

## [0.1.96] - 2021-11-09

[Assistant.NET Release 0.1.96](https://github.com/iotbusters/assistant.net/releases/tag/0.1.96)

### 0.1.96 Changed

- split `MongoOptions` for server/client by name

## [0.1.95] - 2021-11-09

[Assistant.NET Release 0.1.95](https://github.com/iotbusters/assistant.net/releases/tag/0.1.95)

### 0.1.95 Changed

- serilizer converter exposes internal json exceptions
- introduced none backward compatible change to messaging client and storage
  - moved database name from client/server options to `MongoOptions`
- added extension methods for configuring `MongoOptions` in messaging client and storage

### 0.1.95 Fixed

- overriding local handlers (after providers recent intruduction)

## [0.1.94] - 2021-11-03

[Assistant.NET Release 0.1.94](https://github.com/iotbusters/assistant.net/releases/tag/0.1.94)

### 0.1.94 Added

- `IMessagingClient.PublishObject` method with fire-and-forget behavior which doesn't wait for a response

### 0.1.94 Changed

- moved messaging client extensions to `Abstractions` namespace to remove extra using
- renamed `IMessagingClient` method `SendObject` to `RequestObject`
- renamed `IMessagingClient` related extension methods
- introduced `IMessageHandlingProvider` abstraction to isolate data provider for messaging client

## [0.1.93] - 2021-10-31

[Assistant.NET Release 0.1.93](https://github.com/iotbusters/assistant.net/releases/tag/0.1.93)

### 0.1.93 Added

- instance based interceptors support in messaging client

### 0.1.93 Changed

- introduced none backward compatible change to messaging client
  - isolated `IMessageHandler` interfaces from `IAbstractHandler` to fix multiple interface implementation issue
  - isolated `IMessageInterceptor` interfaces from `IAbstractInterceptor` to fix multiple interface implementation issue
  - removed type constrains during handler and interceptor registrations (but added runtime validation instead)
  - decomposed interceptor cascade calls to reduce generated stacktrace because of chain of lambdas

## [0.1.92] - 2021-10-25

[Assistant.NET Release 0.1.92](https://github.com/iotbusters/assistant.net/releases/tag/0.1.92)

### 0.1.92 Added

- `storage` configuration objects

### 0.1.92 Changed

- introduced none backward compatible change to messaging client
  - mongo and web provider configurations were completely changed for server implementations
- `IMongoClient` configuration is isolated between different providers (`storage` and `messaging`)

## [0.1.91] - 2021-10-18

[Assistant.NET Release 0.1.91](https://github.com/iotbusters/assistant.net/releases/tag/0.1.91)

### 0.1.91 Added

- type extension methods for messages and handlers

### 0.1.91 Changed

- introduced none backward compatible change to messaging client
  - renamed local message handler registration methods
- audit models in `Messaging` and `Storage`
- remote handling registration to support only registered messages on a server for WEB and MongoDB providers

## [0.1.90] - 2021-10-18

[Assistant.NET Release 0.1.90](https://github.com/iotbusters/assistant.net/releases/tag/0.1.90)

### 0.1.90 Added

- `ISystemLifetime.RequestStopping` property
- `IDiagnosticContext.CorrelationId` property
- `IDiagnosticContext.User` property
- instance based message handler registration in `Messaging`
- default interceptors configuration in `Messaging`
- logging in interceptors in `Messaging`
- retrying strategies (used in `Messaging` and `Storage`)
- MongoDB providers (`Messaging.Mongo*` and `Storage.Mongo`)
- `Decorate()`  overloaded extension methods
- `IHistoricalStorage` implementation
- auditing information for stored data in `Storage`

### 0.1.90 Changed

- refactored `HashExtensions` (impacting `Messaging` and `Storage`)
- updated diagnostic context registration in `ServiceCollectionExtensions`
- refactored exception serialization in `Messaging.Web`
- refactored `Messaging.Web.Client` package
- renamed `AddRemoteWebMessageHandler()` to `AddWebMessageHandling()`

### 0.1.90 Fixed

- fixed `CachingInterceptor`
- fixed `DeferredCachingInterceptor`
- removed implicit scopes from `IMessagingClient` because of loosing original scope (impacted `IDiagnosticContext` usages)
- fixed `IDiagnosticContext` registration
- fixed `Decorate()` extension method

## [0.1.88] - 2021-09-28

[Assistant.NET Release 0.1.88](https://github.com/iotbusters/assistant.net/releases/tag/0.1.88)

### 0.1.88 Changed

- introduced none backward compatible change to messaging client
  - renamed its operations including extensions

### 0.1.88 Fixed

- default value for reading partition from partitioned storage

## [0.1.86] - 2021-09-23

[Assistant.NET Release 0.1.86](https://github.com/iotbusters/assistant.net/releases/tag/0.1.86)

### 0.1.86 Added

- private ctor support in advanced json serialization

### 0.1.86 Changes

- improved type encoder

### 0.1.86 Fixed

- middleware resolving issues in remote web message handling
- infinite loop in message client
- caching interceptor

### 0.1.86 Removed

- some remote message server handling duplicates
- requirement to handle only message requests on a configured host

## [0.1.78] - 2021-09-20

[Assistant.NET Release 0.1.78](https://github.com/iotbusters/assistant.net/releases/tag/0.1.78)

### 0.1.78 Added

- added mongo storage provider

### 0.1.78 Changed

- introduced none backward compatible change to remote messaging
- local storage refactoring

## [0.1.72] - 2021-09-08

[Assistant.NET Release 0.1.72](https://github.com/iotbusters/assistant.net/releases/tag/0.1.72)

### 0.1.72 Changed

- refactored `Serialization.Json` to support polymorphic and generic value types
- refactored key-value storing mechanism in `Storage`

### Fixed 0.1.72

- caching interceptor issues related to serialization in `Storage`

## [0.1.71] - 2021-09-02

[Assistant.NET Release 0.1.71](https://github.com/iotbusters/assistant.net/releases/tag/0.1.71)

### 0.1.71 Changed

- introduced none backward compatible change to serialization and storage related packages

## [0.1.70] - 2021-09-02

[Assistant.NET Release 0.1.70](https://github.com/iotbusters/assistant.net/releases/tag/0.1.70)

### 0.1.70 Added

- introduced none backward compatible change to `Messaging*` packages
  - added cancellation token to messaging async operations

### 0.1.70 Deprecated

- task mapping extensions

## [0.1.69] - 2021-09-01

[Assistant.NET Release 0.1.69](https://github.com/iotbusters/assistant.net/releases/tag/0.1.69)

### 0.1.69 Fixed

- fixed local storage lifetime issue

## [0.1.68] - 2021-08-25

[Assistant.NET Release 0.1.68](https://github.com/iotbusters/assistant.net/releases/tag/0.1.68)

### 0.1.68 Changed

- introduced none backward compatible change to messaging related packages
  - `command` related files were renamed to `message` to avoid ambiguity with patterns

## [0.1.65] - 2021-08-17

[Assistant.NET Release 0.1.65](https://github.com/iotbusters/assistant.net/releases/tag/0.1.65)

### 0.1.65 Changed

- code docs

### 0.1.65 Fixed

- bug fixes

### 0.1.65 Removed

- unused files

## [0.1.59] - 2021-08-03

[Assistant.NET Release 0.1.59](https://github.com/iotbusters/assistant.net/releases/tag/0.1.59)

### 0.1.59 Changed

- proxy generation packages

## [0.1.42] - 2021-06-29

[Release 0.1.42](https://github.com/iotbusters/assistant.net/releases/tag/0.1.42)

### 0.1.42 Added

- `Serialization.Json` package

### 0.1.42 Changed

- refactoring

### 0.1.42 Fixed

- bug fixes

## [0.1.40] - 2021-06-03

[Release 0.1.40](https://github.com/iotbusters/assistant.net/releases/tag/0.1.40)

### 0.1.40 Added

- partitioned `Storage`

### 0.1.40 Changed

- refactoring

### 0.1.40 Fixed

- bug fixes