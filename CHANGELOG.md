# Assistant.NET Changelog

All relevant changes to packages which were released or being prepared for releasing.

See also [keepachangelog.com](https://keepachangelog.com/en/1.0.0/).

## 0.1.90 - 2021-10-18

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

## 0.1.88 - 2021-09-28

[Assistant.NET Release 0.1.88](https://github.com/iotbusters/assistant.net/releases/tag/0.1.88)

### 0.1.88 Changed

- introduced none backward compatible change to messaging client
  - renamed its operations including extensions

### 0.1.88 Fixed

- default value for reading partition from partitioned storage

## 0.1.86 - 2021-09-23

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

## 0.1.78 - 2021-09-20

[Assistant.NET Release 0.1.78](https://github.com/iotbusters/assistant.net/releases/tag/0.1.78)

### 0.1.78 Added

- added mongo storage provider

### 0.1.78 Changed

- introduced none backward compatible change to remote messaging
- local storage refactoring

## 0.1.72 - 2021-09-08

[Assistant.NET Release 0.1.72](https://github.com/iotbusters/assistant.net/releases/tag/0.1.72)

### 0.1.72 Changed

- refactored `Serialization.Json` to support polymorphic and generic value types
- refactored key-value storing mechanism in `Storage`

### Fixed 0.1.72

- caching interceptor issues related to serialization in `Storage`

## 0.1.71 - 2021-09-02

[Assistant.NET Release 0.1.71](https://github.com/iotbusters/assistant.net/releases/tag/0.1.71)

### 0.1.71 Changed

- introduced none backward compatible change to serialization and storage related packages

## 0.1.70 - 2021-09-02

[Assistant.NET Release 0.1.70](https://github.com/iotbusters/assistant.net/releases/tag/0.1.70)

### 0.1.70 Added

- introduced none backward compatible change to `Messaging*` packages
  - added cancellation token to messaging async operations

### 0.1.70 Deprecated

- task mapping extensions

## 0.1.69 - 2021-09-01

[Assistant.NET Release 0.1.69](https://github.com/iotbusters/assistant.net/releases/tag/0.1.69)

### 0.1.69 Fixed

- fixed local storage lifetime issue

## 0.1.68 - 2021-08-25

[Assistant.NET Release 0.1.68](https://github.com/iotbusters/assistant.net/releases/tag/0.1.68)

### 0.1.68 Changed

- introduced none backward compatible change to messaging related packages
  - `command` related files were renamed to `message` to avoid ambiguity with patterns

## 0.1.65 - 2021-08-17

[Assistant.NET Release 0.1.65](https://github.com/iotbusters/assistant.net/releases/tag/0.1.65)

### 0.1.65 Changed

- code docs

### 0.1.65 Fixed

- bug fixes

### 0.1.65 Removed

- unused files

## 0.1.59 - 2021-08-03

[Assistant.NET Release 0.1.59](https://github.com/iotbusters/assistant.net/releases/tag/0.1.59)

### 0.1.59 Changed

- proxy generation packages

## 0.1.42 - 2021-06-29

[Release 0.1.42](https://github.com/iotbusters/assistant.net/releases/tag/0.1.42)

### 0.1.42 Added

- `Serialization.Json` package

### 0.1.42 Changed

- refactoring

### 0.1.42 Fixed

- bug fixes

## 0.1.40 - 2021-06-03

[Release 0.1.40](https://github.com/iotbusters/assistant.net/releases/tag/0.1.40)

### 0.1.40 Added

- partitioned `Storage`

### 0.1.40 Changed

- refactoring

### 0.1.40 Fixed

- bug fixes