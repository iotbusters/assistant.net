using System;
using Microsoft.Extensions.DependencyInjection;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;
using Assistant.Net.Diagnostics.Internal;

namespace Assistant.Net.Diagnostics
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers default diagnostics context.
        /// </summary>
        public static IServiceCollection AddDiagnosticsContext(this IServiceCollection services) => services
            .TryAddScoped(p => new DiagnosticsContext())
            .TryAddScoped(InitializeWith(p => Guid.NewGuid()));

        /// <summary>
        ///     Registers diagnostics context customized by predicate <paramref name="getCorrelationId" />.
        /// </summary>
        public static IServiceCollection AddDiagnosticsContext(this IServiceCollection services, Func<IServiceProvider, Guid> getCorrelationId) => services
            .TryAddScoped(p => new DiagnosticsContext())
            .ReplaceScoped(InitializeWith(getCorrelationId));

        /// <summary>
        ///     Registers diagnostics services.
        /// </summary>
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