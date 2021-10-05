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
            .TryAddScoped<IDiagnosticContext>(_ => new DiagnosticContext(Guid.NewGuid().ToString()));

        /// <summary>
        ///     Registers diagnostic context customized by a function <paramref name="getCorrelationId" />.
        /// </summary>
        public static IServiceCollection AddDiagnosticContext(this IServiceCollection services, Func<IServiceProvider, string> getCorrelationId) => services
            .ReplaceScoped<IDiagnosticContext>(p => new DiagnosticContext(getCorrelationId(p)));

        /// <summary>
        ///     Registers custom diagnostic context <typeparamref name="TContext" />.
        /// </summary>
        public static IServiceCollection AddDiagnosticContext<TContext>(this IServiceCollection services)
            where TContext : class, IDiagnosticContext => services
            .TryAddScoped<IDiagnosticContext, TContext>();

        /// <summary>
        ///     Registers diagnostic services.
        /// </summary>
        public static IServiceCollection AddDiagnostics(this IServiceCollection services) => services
            .AddSystemClock()
            .AddDiagnosticContext()
            .TryAddSingleton(_ => OperationEventSource.Instance)
            .TryAddScoped<IDiagnosticFactory, DiagnosticFactory>();
    }
}
