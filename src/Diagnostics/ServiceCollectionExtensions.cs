using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;
using Assistant.Net.Diagnostics.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Diagnostics
{
    /// <summary>
    ///     Service collection extensions for diagnostics.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers default diagnostics context.
        /// </summary>
        public static IServiceCollection AddDiagnosticContext(this IServiceCollection services) => services
            .TryAddScoped(_ => new DiagnosticContext())
            .TryAddScoped(InitializeWith(_ => Guid.NewGuid().ToString()));

        /// <summary>
        ///     Registers diagnostic context customized by a function <paramref name="getCorrelationId" />.
        /// </summary>
        public static IServiceCollection AddDiagnosticContext(this IServiceCollection services, Func<IServiceProvider, string> getCorrelationId) => services
            .TryAddScoped(_ => new DiagnosticContext())
            .ReplaceScoped(InitializeWith(getCorrelationId));

        /// <summary>
        ///     Registers diagnostic services.
        /// </summary>
        public static IServiceCollection AddDiagnostics(this IServiceCollection services) => services
            .AddSystemClock()
            .AddDiagnosticContext()
            .TryAddSingleton(_ => OperationEventSource.Instance)
            .TryAddScoped<IDiagnosticFactory, DiagnosticFactory>();

        private static Func<IServiceProvider, IDiagnosticContext> InitializeWith(Func<IServiceProvider, string> getCorrelationId) =>
            p =>
            {
                var context = p.GetRequiredService<DiagnosticContext>();
                context.CorrelationId = getCorrelationId(p);
                return context;
            };
    }
}
