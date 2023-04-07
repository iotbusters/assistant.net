# assistant.net.serialization

Common serializer implementation extensible for using different type/format that can be easily extended for new formats
including theirs own specific tuning extensions. It gives an opportunuty to isolate an application from specific serialization format
and manage it centralizely.

## Default configuration

To add serialization one of methods should be called:

```csharp
var services = new ServiceCollection()
    .AddSerializer(b => ...) // adds dependencies and initial configuration
    .ConfigureSerializer(b => ...); // appends existing configuration (no dependencies are added)
```

where a specific format should be selected for serialization

```csharp
services.ConfigureSerializer(b => b.UseFormat()); // e.g. JSON or others

```
and types allowed for serialization

```csharp

services.ConfigureSerializer(b => b
    .AddType<SomeModel>() // by registering specific types
    .AllowAnyType()); // or allowing any type

```

## Named configuration

Serializer implementation is based on
[named options](https://github.com/iotbusters/assistant.net/blob/master/Core/README.md#named-options)
so you can have multiple named serializers with different configurations.

```csharp
var provider = new ServiceCollection()
    .AddSerializer()
    .ConfigureSerializer("name-1", o => o.UseJson().AddType<DateTime>())
    .ConfigureSerializer("name-2", o => o.UseProto().AddType<TimeSpan>())
    .BuildServiceProvider();

using var scope1 = provider.CreateScopeWithNamedOptionContext("name-1");
using var scope2 = provider.CreateScopeWithNamedOptionContext("name-2");

var serializer1 = scope1.ServiceProvider.GetRequiredService<ISerializer<DateTime>>();
var serializer2 = scope2.ServiceProvider.GetRequiredService<ISerializer<TimeSpan>>();
```

## Serializer resolving

Once seralization was properly configured a specific serializer can be resolved directly

```csharp
var serializer1 = provider.GetRequiredService<ISerializer<SomeModel>>();
var serializer2 = provider.GetRequiredService<ISerializer<AnotherModel>>();
```

or via serialization factory

```csharp
var factory = provider.GetRequiredService<ISerializerFactory>();
var serializer1 = factory.Create(typeof(SomeModel));
var serializer2 = factory.Create<AnotherModel>();
```

Pay attention, if serialization isn't properly configured, resolving a serializer will throw an exception.

## Available formats

- [JSON](https://www.nuget.org/packages/assistant.net.serialization.json/)
- [Protocol Buffer](https://www.nuget.org/packages/assistant.net.serialization.proto/)
