# assistant.net.core

Abstractions, basic implementations, useful tools and extensions which are commonly used across the solution.

## System clock

```csharp
using var provider = new ServiceCollection().AddSystemClock().BuildServiceProvider();

var now = provider.GetRequiredService<ISystemClock>().UtcNow;
```

## System lifetime

The feature exposes access to running application lifetime to have more flexible access over
application shutting down process.

It's cloned from the same purpose implementation used for .net hosted services although without respective dependencies
so that it can be used in .net standard or SDK packages with the same purpose.

```csharp
using var provider = new ServiceCollection().AddSystemLifetime().BuildServiceProvider();

var stoppingCancellationToken = provider.GetRequiredService<ISystemLifetime>().Stopping;
```

## Type encoder

A tool that converts type to string and resolves it back if it's available in current application domain.

Alternative to .net default full assembly name although it ignores type assembly name/version and namespace.

```csharp
using var provider = new ServiceCollection()
    .AddTypeEncoder(o => o.Exclude("NUnit").Exclude<DateTime>())
    .BuildServiceProvider();

var encoder = provider.GetRequiredService<ITypeEncoder>();

var typeName = encoder.Encode(typeof(SomeType));
var type = encoder.Decode(typeName);
```

## Unions

```csharp
Option<int> some = Option.Some(123);
Option<int> none = Option.None;
```

## Named options

Scoped based named options configured to have specific option name within current service provider scope.
It's widely used in named packages like serialization, storage, messaging.

Initializing scope with predefined name:

```csharp
using var provider = new ServiceCollection()
    .AddNamedOptionsContext()
    .Configure<SomeOptions>("name", o => o.Value = 123)
    .BuildServiceProvider();
using var scope = provider.CreateScopeWithNamedOptionContext("name");

var options = scope.ServiceProvider.GetRequiredService<INamedOptions<SomeOptions>>().Value;
```

Override the name within existing scope:

```csharp
using var provider = new ServiceCollection()
    .AddNamedOptionsContext()
    .Configure<SomeOptions>("name", o => o.Value = 123)
    .BuildServiceProvider();
using var scope = provider.CreateScope();
scope.ConfigureNamedOptionContext("name");

var options = scope.ServiceProvider.GetRequiredService<INamedOptions<SomeOptions>>().Value;
```

## Configure options source

The .net options feature extension that gives an opportunity to reload specific options configuration
including own configuration by some custom logic. E.g. options is dynamically populated during runtime.

This is an alternative solution to 'IConfigurationProvider' feature although it can be part of existing DI
and it configures directly the options object instead of indirect `IConfigurationRoot`.

```csharp
var source = new CustomConfigureOptionsSource();
using var provider = new ServiceCollection()
    .AddSingleton(source)
    .Configure<SomeOptions>("name", o => o.Value = 123)
    .BindOptions("name", source)
    .BuildServiceProvider();

var monitor = provider.GetRequiredService<IOptionsSnapshot<SomeOptions>>();
var source2 = provider.GetRequiredService<CustomConfigureOptionsSource>();

source2.Reload(o => o.Value = 321);

var options = monitor.Get<SomeOptions>("name");

internal class CustomConfigureOptionsSource : IConfigureOptionsSource { ... }
```

## Configure options with validation

Just override of original `Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions` extension methods
with enabled data annotation validation.

```csharp
using Assistant.Net;

using var provider = new ServiceCollection()
    .Configure<SomeOptions>(delegate { })
    .BuildServiceProvider();
```

## Hash code

Simple extension methods for generating SHA1 hash code.

```csharp
string byteCode = new byte[0].GetSha1();
string stringCode = "".GetSha1();
string structCode = 123.GetSha1();
string objectCode = new SomeObject().GetSha1();
```

## Provider configuration extensions

- [assistant.net.mongo](https://www.nuget.org/packages/assistant.net.mongo/)
- [assistant.net.sqlite](https://www.nuget.org/packages/assistant.net.sqlite/)