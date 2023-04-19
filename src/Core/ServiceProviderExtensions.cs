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
    ///     Clone a scope with configured <see cref="NamedOptionsContext"/>.
    /// </summary>
    /// <param name="provider"/>
    public static IServiceScope CloneScopeWithNamedOptionContext(this IServiceProvider provider)
    {
        var context = provider.GetService<NamedOptionsContext>();
        if (context == null)
            throw new ArgumentException("No named option contest is registered.", nameof(provider));

        return provider.GetRequiredService<IServiceScopeFactory>().CreateScopeWithNamedOptionContext(context.Name);
    }

    /// <summary>
    ///     Clone an async scope with configured <see cref="NamedOptionsContext"/>.
    /// </summary>
    /// <param name="provider"/>
    public static AsyncServiceScope CloneAsyncScopeWithNamedOptionContext(this IServiceProvider provider)
    {
        var context = provider.GetService<NamedOptionsContext>();
        if (context == null)
            throw new ArgumentException("No named option contest is registered.", nameof(provider));

        return provider.GetRequiredService<IServiceScopeFactory>().CreateAsyncScopeWithNamedOptionContext(context.Name);
    }

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
    /// <param name="scope"/>
    /// <param name="name">The name of options instance.</param>
    public static IServiceScope ConfigureNamedOptionContext(this IServiceScope scope, string name)
    {
        scope.ServiceProvider.ConfigureNamedOptionContext(name);
        return scope;
    }

    /// <summary>
    ///     Configures <see cref="NamedOptionsContext"/> of current scope.
    /// </summary>
    /// <param name="scope"/>
    /// <param name="name">The name of options instance.</param>
    public static AsyncServiceScope ConfigureNamedOptionContext(this AsyncServiceScope scope, string name)
    {
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
