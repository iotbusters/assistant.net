# assistant.net.serialization.json

Generic serialization mechanism with flexible type/format configuration.
The only implementation for JSON for now. Further it will be extended with other formats, e.g. protobuf.

## Typed serializer

```csharp
using var provider = new ServiceCollection()
    .AddSerializer(b => b
        .AddJsonType<SomeModel>()
        .AddJsonTypeAny())
    .BuildServiceProvider();

var serializer1 = provider.GetRequiredService<ISerializer<SomeModel>>();
var serializer2 = provider.GetRequiredService<ISerializer<AnotherModel>>();
```

## Serializer factory

```csharp
using var provider = new ServiceCollection()
    .AddSerializer(b => b
        .AddJsonType<SomeModel>()
        .AddJsonTypeAny())
    .BuildServiceProvider();

var factory = provider.GetRequiredService<ISerializerFactory>();
var objectSerializer1 = factory.Create(typeof(SomeModel));
var objectSerializer1 = factory.Create(typeof(AnotherModel));
```

## Configuration

```csharp
using var provider = new ServiceCollection()
    .AddSerializer(delegate { })
    .ConfigureSerializer(delegate { })
    .ConfigureJsonOptions(delegate { })
    .BuildServiceProvider();

var factory = provider.GetRequiredService<ISerializerFactory>();
var objectSerializer1 = factory.Create(typeof(SomeModel));
var objectSerializer1 = factory.Create(typeof(AnotherModel));
```
