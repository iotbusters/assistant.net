# assistant.net.serialization.json

JSON format serializer implementation of [assistant.net.serialization](https://github.com/iotbusters/assistant.net/tree/master/src/Serialization)
based on `System.Text.Json`.

## Configuration

JSON configuration sample

```csharp
services.AddSerializer(b => b
    .UseJson() // required
    .AddJsonConverter<CustomJsonConverter>()); // optional
```

`System.Text.Json` backed serializer can be configured separately

```csharp
services.ConfigureJsonSerializerOptions(o => o.Converters.TryAdd(new CustomJsonConverter()));
```
