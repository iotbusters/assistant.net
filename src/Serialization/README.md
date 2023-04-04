# assistant.net.serialization

Generic serialization mechanism with flexible type/format configuration.
See specific format implementation packages like [assistant.net.serialization.*](https://www.nuget.org/packages?q=assistant.net.serialization.).

## Configuration

Serialization can be configured by calling one of methods:

```csharp
var services = new ServiceCollection()
    .AddSerializer(b => ...)
    .ConfigureSerializer(b => ...);
```

which can select a format to serialize with

```csharp
services.ConfigureSerializer(b => b.UseJson()); // or other.

```
and types allowed for serialization

```csharp

services.ConfigureSerializer(b => b
    .AddType<SomeModel>() // by registering specific types
    .AllowAnyType()); // or allowing any type

```

## Serializer resolving

Once seralization was properly configured a typed serializer can be resolved directly

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

Pay attention, if serialization of a type wasn't configured, serializer resolving will throw an exception.