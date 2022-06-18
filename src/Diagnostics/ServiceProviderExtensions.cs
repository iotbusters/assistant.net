using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Diagnostics;

/// <summary>
///     Service provider extensions for diagnostics.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    ///     Configures <see cref="DiagnosticContext"/> of current scope.
    /// </summary>
    /// <param name="provider"/>
    /// <param name="correlationId">Correlation ID to be set.</param>
    /// <param name="user">User (ID/Name) to be set.</param>
    public static IServiceProvider ConfigureDiagnosticContext(this IServiceProvider provider, string? correlationId = null, string? user = null)
    {
        var context = provider.GetRequiredService<DiagnosticContext>();
        context.CorrelationId = correlationId;
        context.User = user;
        return provider;
    }

    /// <summary>
    ///     Configures <see cref="DiagnosticContext"/> of current scope.
    /// </summary>
    /// <param name="scope"/>
    /// <param name="correlationId">Correlation ID to be set.</param>
    /// <param name="user">User (ID/Name) to be set.</param>
    public static IServiceScope ConfigureDiagnosticContext(this IServiceScope scope, string? correlationId = null, string? user = null)
    {
        var context = scope.ServiceProvider.GetRequiredService<DiagnosticContext>();
        context.CorrelationId = correlationId;
        context.User = user;
        return scope;
    }

    /// <summary>
    ///     Configures <see cref="DiagnosticContext"/> of current scope.
    /// </summary>
    /// <param name="scope"/>
    /// <param name="correlationId">Correlation ID to be set.</param>
    /// <param name="user">User (ID/Name) to be set.</param>
    public static AsyncServiceScope ConfigureDiagnosticContext(this AsyncServiceScope scope, string? correlationId = null, string? user = null)
    {
        var context = scope.ServiceProvider.GetRequiredService<DiagnosticContext>();
        context.CorrelationId = correlationId;
        context.User = user;
        return scope;
    }
}
