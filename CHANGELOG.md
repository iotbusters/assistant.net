# Assistant.NET Changelog

All relevant changes to packages which were released or being prepared for releasing.

See also [keepachangelog.com](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Changed

- nothing

## [0.3.138] - 2022-08-07

[Assistant.NET Release 0.3.138](https://github.com/iotbusters/assistant.net/releases/tag/0.3.138)
[Changes](https://github.com/iotbusters/assistant.net/compare/0.3.137...0.3.138)

### 0.3.138 Added

- expiration of requested message handling

### 0.3.138 Changed

- extended retry strategy abstraction with total delay time
- extended healthcheck registration methods

### 0.3.138 Fixed

- health check cleanup after changing messaging handling provider on a server
- accepting not configured messages on a server

## [0.3.137] - 2022-08-06

[Assistant.NET Release 0.3.137](https://github.com/iotbusters/assistant.net/releases/tag/0.3.137)
[Changes](https://github.com/iotbusters/assistant.net/compare/0.3.135...0.3.137)

### 0.3.137 Added

- mongo and sqlite health check projects
- added created/updated properties to detailed storage value
- messaging server health checks
- publishing server accepting messages based on health checks

### 0.3.137 Changed

- core extension methods
- remote server selection for message handling based on published servers

## [0.3.135] - 2022-07-14

[Assistant.NET Release 0.3.135](https://github.com/iotbusters/assistant.net/releases/tag/0.3.135)
[Changes](https://github.com/iotbusters/assistant.net/compare/0.3.133...0.3.135)

### 0.3.135 Added

- storage mongo and sqlite provider database initialization

### 0.3.135 Changed

- improved performance of storage mongo and sqlite providers

### 0.3.135 Removed

- hosted service based storage provider database configuration from messaging mongo and sqlite providers

## [0.3.133] - 2022-07-04

[Assistant.NET Release 0.3.133](https://github.com/iotbusters/assistant.net/releases/tag/0.3.133)
[Changes](https://github.com/iotbusters/assistant.net/compare/0.3.128...0.3.133)

### 0.3.133 Added

- async enumerable extension methods
- operations for detailed storage values
- operations for removing serializing types

### 0.3.133 Changed

- hidden shared abstract interceptor from exception stacktrace
- renamed intercept method of shared abstract interceptor
- improved logging in messaging
- improved sha1 hash code calculation
- message handling coordination mechanism

### 0.3.133 Fixed

- cancellation token based unsafe delays
- configuration options source binding extension methods
- debug timeout configuration method in messaging
- implicit caching all message responses

### 0.3.133 Removed

- some of task related union options extension methods

## [0.3.128] - 2022-06-29

[Assistant.NET Release 0.3.128](https://github.com/iotbusters/assistant.net/releases/tag/0.3.128)
[Changes](https://github.com/iotbusters/assistant.net/compare/0.3.127...0.3.128)

### 0.3.128 Added

- extension methods for registering configuration change token source
- request/publish interceptor interfaces
- introduced shared publish/request interceptor base class

### 0.3.128 Fixed

- publish method in messaging client

### 0.3.128 Removed

- single request/publish interceptor interface

## [0.3.127] - 2022-06-26

[Assistant.NET Release 0.3.127](https://github.com/iotbusters/assistant.net/releases/tag/0.3.127)

### 0.3.127 Added

- exception stacktrace to json serializer

### 0.3.127 Changed

- hide exceptino related extension methods and internal irrelevant methods
- improved interceptor returning task to value task
- messaging registration extension methods

## [0.3.125] - 2022-06-25

[Assistant.NET Release 0.3.125](https://github.com/iotbusters/assistant.net/releases/tag/0.3.125)

### 0.3.125 Fixed

- get keys issue in sqlite storage provider
- missing cancellation token to storage extension method

## [0.3.123] - 2022-06-23

[Assistant.NET Release 0.3.123](https://github.com/iotbusters/assistant.net/releases/tag/0.3.123)

### 0.3.123 Added

- delayed cancellation interceptor
- time validation attribute

### 0.3.123 Changed

- type encoder become reloadable
- renamed no-response class to void conflicts with unions namespace
- allowed exposing operation cancelled exception from storage

## [0.3.121] - 2022-06-22

[Assistant.NET Release 0.3.121](https://github.com/iotbusters/assistant.net/releases/tag/0.3.121)

### 0.3.121 Added

- message interceptors implemented 'IAbstractInterceptor' support in messaging

### 0.3.121 Changed

- renamed non-caching message marker interface to `INonCaching` in messaging
- moved message caching responsibility into `MessageHandler` in messaging
- messaging logging

### 0.3.121 Fixed

- predefined message interceptors to support value typed message responses in messaging

## [0.3.119] - 2022-06-20

[Assistant.NET Release 0.3.119](https://github.com/iotbusters/assistant.net/releases/tag/0.3.119)

### 0.3.119 Added

- static types filtering in type encoder
- extension method for disabling timeout interceptor when debugger is attached

### 0.3.119 Changed

- moved response storing to interceptor for generic messaging server
- small refactoring to web and generic messaging clients
- storage and messaging logging

### 0.3.119 Fixed

- conflicting key between different storage value types issue

## [unlisted][0.2.117] - 2022-06-18

[Assistant.NET Release 0.2.117](https://github.com/iotbusters/assistant.net/releases/tag/0.2.117)

### 0.2.117 Added

- type encoder configuration
- diagnostics configuration extension methods
- missed message type acceptance filtering in generic messaging server

## [unlisted][0.2.116] - 2022-06-17

[Assistant.NET Release 0.2.116](https://github.com/iotbusters/assistant.net/releases/tag/0.2.116)

### 0.2.116 Changed

- configure options source registration as scoped
- change-on-options registration as scoped

### 0.2.116 Fixed

- change-on-options feature issues

### 0.2.116 Removed

- disposing db context in sqlite storage provider which missed

## [unlisted][0.2.115] - 2022-06-15

[Assistant.NET Release 0.2.115](https://github.com/iotbusters/assistant.net/releases/tag/0.2.115)

### 0.2.115 Added

- async/sync enumerable extensions
- registering handler instance extension method in messaging generic server

### 0.2.115 Changed

- storage type of caching interceptor in messaging

### 0.2.115 Fixed

- validation message duplicates generated by data annotation options validating
- interceptor duplicates (only one per message type)
- restoring saved processed message handling index in messaging generic server
- extension methods naming in messaging generic server

### 0.2.115 Removed

- unused custom caching code abstraction
- processed messages cleanup in messaging generic server
- disposing db context in sqlite storage provider

## [unlisted][0.2.114] - 2022-06-01

[Assistant.NET Release 0.2.114](https://github.com/iotbusters/assistant.net/releases/tag/0.2.114)

### 0.2.114 Added

- added new provider use* extensions for storage, client and server providers
- standalone generic (storage based) messaging client/server templates

### 0.2.114 Changed

- mongo and sqlite client/server implementations to be based on generic templates

### 0.2.114 Removed

- generic builder abstraction and extensions

## [unlisted][0.2.113] - 2022-06-06

[Assistant.NET Release 0.2.113](https://github.com/iotbusters/assistant.net/releases/tag/0.2.113)

### 0.2.113 Added

- builder abstraction for provider specific configuration methods
- common mongo/sqlite configuration extension methods
- options and builder configuration extension methods
- named options based on scoped context and registration extensions
- named serialization, storage configuration
- single provider feature for storage and messaging

### 0.2.113 Changed

- serializer, storage, messaging configuration
- moved primitive storage value converters to configuration

### 0.2.113 Fixed

- named serializer, storage, messaging configuration

### 0.2.113 Removed

- messaging client builder derived classes
- default MongoDB database name
- mongo client factory

## [unlisted][0.2.109] - 2022-05-22

[Assistant.NET Release 0.2.109](https://github.com/iotbusters/assistant.net/releases/tag/0.2.109)

### 0.2.109 Added

- message caching controlling interfaces accepted by caching interceptor

## [unlisted][0.2.108] - 2022-05-20

[Assistant.NET Release 0.2.108](https://github.com/iotbusters/assistant.net/releases/tag/0.2.108)

### 0.2.108 Added

- sqlite storage and messaging providers

### 0.2.108 Changed

- mongo messaging providers to use mongo storage instead of custom implementation

### 0.2.108 Fixed

- bugs in messaging client
  - interceptor invalid ordering
  - interceptor invalid options injecting
  - storage invalid options registering
- configure options source extension method

### 0.2.108 Removed

- messaging web/mongo packages

## [unlisted][0.2.105] - 2022-05-05

[Assistant.NET Release 0.2.105](https://github.com/iotbusters/assistant.net/releases/tag/0.2.105)

### 0.2.105 Added

- options binding extensions

### 0.2.105 Changed

- refactored mongo storage providers

### 0.2.105 Fixed

- issues with options binding

## [unlisted][0.2.103] - 2022-02-13

[Assistant.NET Release 0.2.103](https://github.com/iotbusters/assistant.net/releases/tag/0.2.103)

### 0.2.103 Changed

- refactored options dependency mechanism

### 0.2.103 Fixed

- issues with options

## [unlisted][0.2.102] - 2021-12-20

[Assistant.NET Release 0.2.102](https://github.com/iotbusters/assistant.net/releases/tag/0.2.102)

### 0.2.102 Changed

- upgraded to .net version 6

## [unlisted][0.2.101] - 2021-12-16

[Assistant.NET Release 0.2.101](https://github.com/iotbusters/assistant.net/releases/tag/0.2.101)

### 0.2.101 Changed

- reworked messaging client and handling configuration builders to hide unsupported options

## [unlisted][0.1.100] - 2021-12-08

[Assistant.NET Release 0.1.100](https://github.com/iotbusters/assistant.net/releases/tag/0.1.100)

### 0.1.100 Changed

- replaced dedicated messaging server options with named messaging client options
- mongo options naming to be compatible with messaging client options naming

## [unlisted][0.1.99] - 2021-12-05

[Assistant.NET Release 0.1.99](https://github.com/iotbusters/assistant.net/releases/tag/0.1.99)

### 0.1.99 Added

- core options source concept to reload options without configuration layer
- named messaging clients

### 0.1.99 Changed

- referenced messaging server options to messaging client options reload

## [unlisted][0.1.98] - 2021-11-13

[Assistant.NET Release 0.1.98](https://github.com/iotbusters/assistant.net/releases/tag/0.1.98)

### 0.1.98 Added

- mongo and web message handlers removal and cleanup during configuration

## [unlisted][0.1.97] - 2021-11-13

[Assistant.NET Release 0.1.97](https://github.com/iotbusters/assistant.net/releases/tag/0.1.97)

### 0.1.97 Added

- local message handlers removal and cleanup during configuration

### 0.1.97 Changed

- message handlers and interceptors registration

### 0.1.97 Removed

- message handling providers

## [unlisted][0.1.96] - 2021-11-09

[Assistant.NET Release 0.1.96](https://github.com/iotbusters/assistant.net/releases/tag/0.1.96)

### 0.1.96 Changed

- split `MongoOptions` for server/client by name

## [unlisted][0.1.95] - 2021-11-09

[Assistant.NET Release 0.1.95](https://github.com/iotbusters/assistant.net/releases/tag/0.1.95)

### 0.1.95 Changed

- serializer converter exposes internal json exceptions
- introduced none backward compatible change to messaging client and storage
  - moved database name from client/server options to `MongoOptions`
- added extension methods for configuring `MongoOptions` in messaging client and storage

### 0.1.95 Fixed

- overriding local handlers (after providers recent introduction)

## [unlisted][0.1.94] - 2021-11-03

[Assistant.NET Release 0.1.94](https://github.com/iotbusters/assistant.net/releases/tag/0.1.94)

### 0.1.94 Added

- `IMessagingClient.PublishObject` method with fire-and-forget behavior which doesn't wait for a response

### 0.1.94 Changed

- moved messaging client extensions to `Abstractions` namespace to remove extra using
- renamed `IMessagingClient` method `SendObject` to `RequestObject`
- renamed `IMessagingClient` related extension methods
- introduced `IMessageHandlingProvider` abstraction to isolate data provider for messaging client

## [unlisted][0.1.93] - 2021-10-31

[Assistant.NET Release 0.1.93](https://github.com/iotbusters/assistant.net/releases/tag/0.1.93)

### 0.1.93 Added

- instance based interceptors support in messaging client

### 0.1.93 Changed

- introduced none backward compatible change to messaging client
  - isolated `IMessageHandler` interfaces from `IAbstractHandler` to fix multiple interface implementation issue
  - isolated `IMessageInterceptor` interfaces from `IAbstractInterceptor` to fix multiple interface implementation issue
  - removed type constrains during handler and interceptor registrations (but added runtime validation instead)
  - decomposed interceptor cascade calls to reduce generated stacktrace because of chain of lambdas

## [unlisted][0.1.92] - 2021-10-25

[Assistant.NET Release 0.1.92](https://github.com/iotbusters/assistant.net/releases/tag/0.1.92)

### 0.1.92 Added

- `storage` configuration objects

### 0.1.92 Changed

- introduced none backward compatible change to messaging client
  - mongo and web provider configurations were completely changed for server implementations
- `IMongoClient` configuration is isolated between different providers (`storage` and `messaging`)

## [unlisted][0.1.91] - 2021-10-18

[Assistant.NET Release 0.1.91](https://github.com/iotbusters/assistant.net/releases/tag/0.1.91)

### 0.1.91 Added

- type extension methods for messages and handlers

### 0.1.91 Changed

- introduced none backward compatible change to messaging client
  - renamed local message handler registration methods
- audit models in `Messaging` and `Storage`
- remote handling registration to support only registered messages on a server for WEB and MongoDB providers

## [unlisted][0.1.90] - 2021-10-18

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

## [unlisted][0.1.88] - 2021-09-28

[Assistant.NET Release 0.1.88](https://github.com/iotbusters/assistant.net/releases/tag/0.1.88)

### 0.1.88 Changed

- introduced none backward compatible change to messaging client
  - renamed its operations including extensions

### 0.1.88 Fixed

- default value for reading partition from partitioned storage

## [unlisted][0.1.86] - 2021-09-23

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

## [unlisted][0.1.78] - 2021-09-20

[Assistant.NET Release 0.1.78](https://github.com/iotbusters/assistant.net/releases/tag/0.1.78)

### 0.1.78 Added

- added mongo storage provider

### 0.1.78 Changed

- introduced none backward compatible change to remote messaging
- local storage refactoring

## [unlisted][0.1.72] - 2021-09-08

[Assistant.NET Release 0.1.72](https://github.com/iotbusters/assistant.net/releases/tag/0.1.72)

### 0.1.72 Changed

- refactored `Serialization.Json` to support polymorphic and generic value types
- refactored key-value storing mechanism in `Storage`

### Fixed 0.1.72

- caching interceptor issues related to serialization in `Storage`

## [unlisted][0.1.71] - 2021-09-02

[Assistant.NET Release 0.1.71](https://github.com/iotbusters/assistant.net/releases/tag/0.1.71)

### 0.1.71 Changed

- introduced none backward compatible change to serialization and storage related packages

## [unlisted][0.1.70] - 2021-09-02

[Assistant.NET Release 0.1.70](https://github.com/iotbusters/assistant.net/releases/tag/0.1.70)

### 0.1.70 Added

- introduced none backward compatible change to `Messaging*` packages
  - added cancellation token to messaging async operations

### 0.1.70 Deprecated

- task mapping extensions

## [unlisted][0.1.69] - 2021-09-01

[Assistant.NET Release 0.1.69](https://github.com/iotbusters/assistant.net/releases/tag/0.1.69)

### 0.1.69 Fixed

- fixed local storage lifetime issue

## [unlisted][0.1.68] - 2021-08-25

[Assistant.NET Release 0.1.68](https://github.com/iotbusters/assistant.net/releases/tag/0.1.68)

### 0.1.68 Changed

- introduced none backward compatible change to messaging related packages
  - `command` related files were renamed to `message` to avoid ambiguity with patterns

## [unlisted][0.1.65] - 2021-08-17

[Assistant.NET Release 0.1.65](https://github.com/iotbusters/assistant.net/releases/tag/0.1.65)

### 0.1.65 Changed

- code docs

### 0.1.65 Fixed

- bug fixes

### 0.1.65 Removed

- unused files

## [unlisted][0.1.59] - 2021-08-03

[Assistant.NET Release 0.1.59](https://github.com/iotbusters/assistant.net/releases/tag/0.1.59)

### 0.1.59 Changed

- proxy generation packages

## [unlisted][0.1.42] - 2021-06-29

[Release 0.1.42](https://github.com/iotbusters/assistant.net/releases/tag/0.1.42)

### 0.1.42 Added

- `Serialization.Json` package

### 0.1.42 Changed

- refactoring

### 0.1.42 Fixed

- bug fixes

## [unlisted][0.1.40] - 2021-06-03

[Release 0.1.40](https://github.com/iotbusters/assistant.net/releases/tag/0.1.40)

### 0.1.40 Added

- partitioned `Storage`

### 0.1.40 Changed

- refactoring

### 0.1.40 Fixed

- bug fixes
