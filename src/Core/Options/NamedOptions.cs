using Assistant.Net.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Options;

internal class NamedOptions<TOptions> : INamedOptions<TOptions> where TOptions : class
{
    private readonly IOptionsMonitor<TOptions> monitor;
    private readonly NamedOptionsContext context;

    public NamedOptions(IServiceProvider provider)
    {
        this.monitor = provider.GetService<IOptionsMonitor<TOptions>>()
                       ?? throw new ArgumentException($"{typeof(TOptions)} wasn't configured.", nameof(provider));
        this.context = provider.GetService<NamedOptionsContext>()
                       ?? throw new ArgumentException($"{typeof(NamedOptionsContext)} wasn't registered.", nameof(provider));
    }

    public string Name => context.Name;

    public TOptions Value => monitor.Get(Name);
}
