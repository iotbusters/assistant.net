using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Abstractions;

/// <summary>
///     Builder marker interface.
/// </summary>
/// <typeparam name="TBuilder">Specific builder type.</typeparam>
public interface IBuilder<out TBuilder>
{
    /// <summary>
    ///     The name of the options instance.
    /// </summary>
    string Name { get; }

    /// <summary/>
    IServiceCollection Services { get; }

    /// <summary>
    ///     Specific builder instance.
    /// </summary>
    TBuilder Instance { get; }
}
