using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.EventSources;
using Assistant.Net.Diagnostics.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Diagnostics;

/// <summary>
///     Service collection extensions for diagnostics.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers default diagnostics context if no other is registered yet.
    /// </summary>
    public static IServiceCollection AddDiagnosticContext(this IServiceCollection services) => services
        .AddDiagnosticContext((_, ctx) => ctx.CorrelationId = Guid.NewGuid().ToString());

    /// <summary>
    ///     Registers diagnostic context customized by the function <paramref name="getCorrelationId" />.
    /// </summary>
    /// <param name="services"/>
    /// <param name="getCorrelationId">The correlation ID generation factory.</param>
    /// <param name="getUser">The user ID generation factory</param>
    public static IServiceCollection AddDiagnosticContext(this IServiceCollection services, Func<IServiceProvider, string> getCorrelationId, Func<IServiceProvider, string>? getUser = null) => services
        .AddDiagnosticContext((p, ctx) =>
        {
            ctx.CorrelationId = getCorrelationId(p);
            ctx.User = getUser?.Invoke(p);
        });

    /// <summary>
    ///     Registers diagnostic context customized by the <paramref name="configureContext" />.
    /// </summary>
    /// <param name="services"/>
    /// <param name="configureContext">The action used to configure the context.</param>
    public static IServiceCollection AddDiagnosticContext(this IServiceCollection services, Action<IServiceProvider, DiagnosticContext> configureContext) => services
        .ReplaceScoped<IDiagnosticContext>(p => p.GetRequiredService<DiagnosticContext>())
        .ReplaceScoped(p =>
        {
            var context = new DiagnosticContext();
            configureContext(p, context);
            return context;
        });

    /// <summary>
    ///     Registers custom diagnostic context <typeparamref name="TContext" />.
    /// </summary>
    public static IServiceCollection AddDiagnosticContext<TContext>(this IServiceCollection services)
        where TContext : class, IDiagnosticContext => services
        .TryAddScoped<TContext>()
        .ReplaceScoped<IDiagnosticContext>(p => p.GetRequiredService<TContext>());

    /// <summary>
    ///     Registers diagnostic services.
    /// </summary>
    public static IServiceCollection AddDiagnostics(this IServiceCollection services) => services
        .AddSystemClock()
        .AddDiagnosticContext()
        .TryAddSingleton(_ => OperationEventSource.Instance)
        .TryAddScoped<IDiagnosticFactory, DiagnosticFactory>();
}
