using Assistant.Net.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net;

/// <summary>
///     Service provider extensions.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    ///     Creates a scope with configured <see cref="NamedOptionsContext"/>.
    /// </summary>
    /// <param name="provider"/>
    /// <param name="name">The name of options instance.</param>
    public static IServiceScope CreateScopeWithNamedOptionContext(this IServiceProvider provider, string name) => provider
        .GetRequiredService<IServiceScopeFactory>().CreateScopeWithNamedOptionContext(name);

    /// <summary>
    ///     Creates an async scope with configured <see cref="NamedOptionsContext"/>.
    /// </summary>
    /// <param name="provider"/>
    /// <param name="name">The name of options instance.</param>
    public static AsyncServiceScope CreateAsyncScopeWithNamedOptionContext(this IServiceProvider provider, string name) => provider
        .GetRequiredService<IServiceScopeFactory>().CreateAsyncScopeWithNamedOptionContext(name);

    /// <summary>
    ///     Creates a scope with configured <see cref="NamedOptionsContext"/>.
    /// </summary>
    /// <param name="factory"/>
    /// <param name="name">The name of options instance.</param>
    public static IServiceScope CreateScopeWithNamedOptionContext(this IServiceScopeFactory factory, string name)
    {
        var scope = factory.CreateScope();
        scope.ServiceProvider.ConfigureNamedOptionContext(name);
        return scope;
    }

    /// <summary>
    ///     Creates an async scope with configured <see cref="NamedOptionsContext"/>.
    /// </summary>
    /// <param name="factory"/>
    /// <param name="name">The name of options instance.</param>
    public static AsyncServiceScope CreateAsyncScopeWithNamedOptionContext(this IServiceScopeFactory factory, string name)
    {
        var scope = factory.CreateAsyncScope();
        scope.ServiceProvider.ConfigureNamedOptionContext(name);
        return scope;
    }

    /// <summary>
    ///     Configures <see cref="NamedOptionsContext"/> of current scope.
    /// </summary>
    /// <param name="provider"/>
    /// <param name="name">The name of options instance.</param>
    public static IServiceProvider ConfigureNamedOptionContext(this IServiceProvider provider, string name)
    {
        var context = provider.GetRequiredService<NamedOptionsContext>();
        context.Name = name;
        return provider;
    }

    /// <summary>
    ///     Creates instance of <typeparamref name="T"/> with dependencies
    ///     from <paramref name="provider"/> and <paramref name="arguments"/>.
    /// </summary>
    public static T Create<T>(this IServiceProvider provider, params object[] arguments) =>
        (T)provider.Create(typeof(T), arguments);

    /// <summary>
    ///     Creates instance of <paramref name="type"/> with dependencies
    ///     from <paramref name="provider"/> and <paramref name="arguments"/>.
    /// </summary>
    public static object Create(this IServiceProvider provider, Type type, params object[] arguments) =>
        ActivatorUtilities.CreateInstance(provider, type, arguments);
}
