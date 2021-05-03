using System;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;
using Assistant.Net.Diagnostics.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Diagnostics
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOperationContext(this IServiceCollection services) => services
            .TryAddScoped(p => new OperationContext())
            .TryAddScoped(InitializeWith(Guid.NewGuid));

        public static IServiceCollection AddOperationContext(this IServiceCollection services, Func<Guid> getCorrelationId) => services
            .TryAddScoped(p => new OperationContext())
            .ReplaceScoped(InitializeWith(getCorrelationId));

        public static IServiceCollection AddDiagnostics(this IServiceCollection services) => services
            .AddSystemClock()
            .AddOperationContext()
            .TryAddSingleton(x => OperationEventSource.Instance)
            .AddScoped<IOperationFactory, OperationFactory>();

        private static Func<IServiceProvider, IOperationContext> InitializeWith(Func<Guid> getCorrelationId) =>
            p =>
            {
                var context = p.GetRequiredService<OperationContext>();
                context.CorrelationId = getCorrelationId();
                return context;
            };
    }
}