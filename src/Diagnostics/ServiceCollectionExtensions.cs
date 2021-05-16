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
        public static IServiceCollection AddDiagnosticContext(this IServiceCollection services) => services
            .TryAddScoped(p => new DiagnosticContext())
            .TryAddScoped(InitializeWith(p => Guid.NewGuid().ToString()));

        /// <summary>
        ///     Registers diagnostic context customized by a predicate <paramref name="getCorrelationId" />.
        /// </summary>
        public static IServiceCollection AddDiagnosticContext(this IServiceCollection services, Func<IServiceProvider, string> getCorrelationId) => services
            .TryAddScoped(p => new DiagnosticContext())
            .ReplaceScoped(InitializeWith(getCorrelationId));

        /// <summary>
        ///     Registers diagnostic services.
        /// </summary>
        public static IServiceCollection AddDiagnostics(this IServiceCollection services) => services
            .AddSystemClock()
            .AddDiagnosticContext()
            .TryAddSingleton(x => OperationEventSource.Instance)
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