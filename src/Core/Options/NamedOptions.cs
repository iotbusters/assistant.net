using Assistant.Net.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Options;

/// <summary>
///     Default implementation of <see cref="INamedOptions{TOptions}"/>.
/// </summary>
public sealed class NamedOptions<TOptions> : INamedOptions<TOptions> where TOptions : class
{
    private readonly IOptionsSnapshot<TOptions> options;
    private readonly NamedOptionsContext context;

    /// <summary/>
    public NamedOptions(IServiceProvider provider)
    {
        this.options = provider.GetService<IOptionsSnapshot<TOptions>>()
                       ?? throw new ArgumentException($"{typeof(TOptions)} wasn't configured.", nameof(provider));
        this.context = provider.GetService<NamedOptionsContext>()
                       ?? throw new ArgumentException($"{typeof(NamedOptionsContext)} wasn't registered.", nameof(provider));
    }

    /// <inheritdoc cref="NamedOptionsContext.Name"/>
    public string Name => context.Name;

    /// <inheritdoc cref="IOptions{TOptions}.Value"/>
    public TOptions Value => options.Get(Name);
}
