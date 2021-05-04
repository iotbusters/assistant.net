using System;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;
using Assistant.Net.Diagnostics.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Diagnostics
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiagnosticsContext(this IServiceCollection services) => services
            .TryAddScoped(p => new DiagnosticsContext())
            .TryAddScoped(InitializeWith(p => Guid.NewGuid()));

        public static IServiceCollection AddDiagnosticsContext(this IServiceCollection services, Func<IServiceProvider, Guid> getCorrelationId) => services
            .TryAddScoped(p => new DiagnosticsContext())
            .ReplaceScoped(InitializeWith(getCorrelationId));

        public static IServiceCollection AddDiagnostics(this IServiceCollection services) => services
            .AddSystemClock()
            .AddDiagnosticsContext()
            .TryAddSingleton(x => OperationEventSource.Instance)
            .TryAddScoped<IDiagnosticsFactory, DiagnosticsFactory>();

        private static Func<IServiceProvider, IDiagnosticsContext> InitializeWith(Func<IServiceProvider, Guid> getCorrelationId) =>
            p =>
            {
                var context = p.GetRequiredService<DiagnosticsContext>();
                context.CorrelationId = getCorrelationId(p);
                return context;
            };
    }
}