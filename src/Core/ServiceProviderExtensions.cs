using Assistant.Net.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net;

public static class ServiceProviderExtensions
{
    public static IServiceScope CreateScopeWithNamedOptionContext(this IServiceProvider provider, string name) => provider
        .GetRequiredService<IServiceScopeFactory>().CreateScopeWithNamedOptionContext(name);


    public static AsyncServiceScope CreateAsyncScopeWithNamedOptionContext(this IServiceProvider provider, string name) => provider
        .GetRequiredService<IServiceScopeFactory>().CreateAsyncScopeWithNamedOptionContext(name);

    public static IServiceScope CreateScopeWithNamedOptionContext(this IServiceScopeFactory factory, string name)
    {
        var scope = factory.CreateScope();
        scope.ServiceProvider.ConfigureNamedOptionContext(name);
        return scope;
    }

    public static AsyncServiceScope CreateAsyncScopeWithNamedOptionContext(this IServiceScopeFactory factory, string name)
    {
        var scope = factory.CreateAsyncScope();
        scope.ServiceProvider.ConfigureNamedOptionContext(name);
        return scope;
    }

    public static IServiceProvider ConfigureNamedOptionContext(this IServiceProvider provider, string name)
    {
        var context = provider.GetRequiredService<NamedOptionsContext>();
        context.Name = name;
        return provider;
    }
}
