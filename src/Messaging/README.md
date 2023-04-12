# assistant.net.messaging

Simple message exchange implementation designed to be extensible and flexible.
It can handle variety tasks like sync/async communication between components, application layers, or distributed applications.

## Default configuration

To add storage one of methods should be called:

```csharp
var services = new ServiceCollection()
    .AddMessagingClient(b => ...) // adds dependencies and initial configuration
    .ConfigureMessagingClient(b => ...); // appends existing configuration (no dependencies are added)
```

where message handlers should be registered like an in-memory handler

```csharp
services.ConfigureMessagingClient(b => b
    .AddHandler<LocalMessageHandler>() // by a type parameter
    .AddHandler(typeof(LocalMessageHandler)) // by a parameter
    .AddHandler(new LocalMessageHandler())); // or an instance
```

In order to have more flexible control over message handling or applying reusable aspects, interceptors can be configured

```csharp
services.ConfigureMessagingClient(b => b
    .AddInterceptor<RequestMessageInterceptor>() // by a type parameter
    .AddInterceptor(typeof(RequestMessageInterceptor)) // by a parameter
    .AddInterceptor(new RequestMessageInterceptor())); // or an instance
```

[Couple interceptors](https://github.com/iotbusters/assistant.net/tree/master/src/Messaging/Interceptors) are
already available out of box. Some of them have dependency to storage package which should be configured with appropriate provider.
For example, default local implementation

```csharp
services.ConfigureMessagingClient(b => b.UseLocal());
// or
services.ConfigureStorage(b => b.UseLocal());
```

> **Note**
> It should be configured for each named messaging client. See [named configuration](#named-configuration) for details.

## Configuration extension

Messaging client configuration may be big and complicated so applying it again and again can be annoying and error-prone.
But message configuration can help to avoid that

```csharp
services.ConfigureMessagingClient(b => b
    .AddConfiguration<CustomMessageConfiguration>() // by a type parameter
    .AddConfiguration(new CustomMessageConfiguration())); // or an instance

public class CustomMessageConfiguration : IMessageConfiguration { ... }
```

## Single message handler configuration

Some sophisticated scenarios may need to separate message type registration from actual handlers in opposite to
[default configuration](#default-configuration) and the single message handler can be used for the purpose.

So a message type can be registered in one place

```csharp
services.ConfigureMessagingClient(b => b
    .AddSingle<SomeMessage>()
    .AddSingle(typeof(AnotherMessage)));
```

and a single message handler can be configured later in another place (or overridden if needed)

```csharp
services.ConfigureMessagingClient(b => b.UseSpecificHandler()); // mongo or other
```

or configure it as a factory

```csharp
services.ConfigureMessagingClient(b => b
    .UseSingleHandler((provider, messageType) => provider.GetRequiredService<CustomSingleHandler>()));
```

## Back off message handler configuration

In case any message type should be handled or other way tracked, back off message handler can be used.
It comes into play if no other handler was registered for a message.

## Named configuration

Messaging client implementation is based on
[named options](https://github.com/iotbusters/assistant.net/blob/master/Core/README.md#named-options)
so you can have multiple named messaging client with different configurations.

```csharp
var provider = new ServiceCollection()
    .AddMessagingClient()
    .ConfigureMessagingClient("name-1", o => o.UseLocal().AddHandler<LocalMessageHandler>())
    .ConfigureMessagingClient("name-2", o => o.UseMongo().AddSingle<SomeMessage>())
    .BuildServiceProvider();

using var scope1 = provider.CreateScopeWithNamedOptionContext("name-1");
using var scope2 = provider.CreateScopeWithNamedOptionContext("name-2");

var client1 = scope1.ServiceProvider.GetRequiredService<IMessagingClient>();
var client2 = scope2.ServiceProvider.GetRequiredService<IMessagingClient>();
```

## Messaging client resolving

Once the client was properly configured it's instance can be resolved

```csharp
var client = provider.GetRequiredService<IMessagingClient>();

 var response = await client.Request(new SomeMessage()); // sends the message and waits for response
 await client.Publish(new EventMessage()); // sends the message and returns immediately
```

## Class definitions

```csharp
// A message that expects response object
public class SomeMessage : IMessage<SomeResponse> { ... }
public class SomeResponse { ... }
// A response less message
public class EventMessage : IMessage { ... }
// A typed message handler
public class LocalMessageHandler : IMessageHandler<SomeMessage, SomeResponse> { ... }
// A handler that handles any message type
public class AnyMessageHandler : IAbstractHandler { ... }
// A message configuration
public class CustomMessageConfiguration : IMessageConfiguration { ... }
// A typed message interceptor for request operations
public class SomeMessageInterceptor : IMessageRequestInterceptor<SomeMessage, SomeResponse> { ... }
// A typed message interceptor for publish operations
public class EventMessageInterceptor : IMessagePublishInterceptor<EventMessage> { ... }
// A interceptor for request operations that intercept any message types
public class AnyMessageRequestInterceptor : IAbstractRequestInterceptor { ... }
// A interceptor for publish operations that intercept any message types
public class AnyPublishMessageInterceptor : IAbstractPublishInterceptor { ... }
```

## Available providers

- Local provider (in-memory, available out of box)
- MongoDB ([client](https://www.nuget.org/packages/assistant.net.messaging.mongo.client/)/[server](https://www.nuget.org/packages/assistant.net.messaging.mongo.server/))
- SQLite ([client](https://www.nuget.org/packages/assistant.net.messaging.sqlite.client/)/[server](https://www.nuget.org/packages/assistant.net.messaging.sqlite.server/))
- WEB ([client](https://www.nuget.org/packages/assistant.net.messaging.web.client/)/[server](https://www.nuget.org/packages/assistant.net.messaging.web.server/))
