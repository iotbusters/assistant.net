# assistant.net.diagnostics

Diagnostics management tools including tracking operations, correlations, event tracing etc.

## Diagnostic context

Execution context exposing scoped correlation and user IDs.

```csharp
using var provider = new ServiceCollection()
    .AddDiagnosticContext()
    .BuildServiceProvider();

var context = provider.GetRequiredService<IDiagnosticContext>();
var correlationId = provider.GetRequiredService<IDiagnosticContext>().CorrelationId;
var userId = provider.GetRequiredService<IDiagnosticContext>().UserId;
```

## Diagnostics

Operation management tool with default implementation that uses .net `Activity` feature behind the scene.

```csharp
using var provider = new ServiceCollection()
    .AddDiagnostics()
    .BuildServiceProvider();

var factory = provider.GetRequiredService<IDiagnosticFactory>();
var operation = factory.Start();

operation.Complete("message");
// operation.Fail("message");
```
