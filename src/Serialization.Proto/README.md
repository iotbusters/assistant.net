# assistant.net.serialization.json

ProtoBuf format serializer implementation of [assistant.net.serialization](https://github.com/iotbusters/assistant.net/tree/master/src/Serialization)
based on [protobuf-net](https://github.com/protobuf-net/protobuf-net).

## Configuration

ProtoBuf configuration sample

```csharp
services.AddSerializer(b => b
    .UseProto());// required
```

ProtoBuf serializer can be optimized during configuration time if serializing type was registered by a dedicated method

```csharp
services.ConfigureSerializer(b => b.AddTypeProto<SomeModel>());
```
