using Assistant.Net.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net.Options;

/// <summary>
///     Custom <see cref="Microsoft.Extensions.Options.OptionsBuilder{TOptions}"/> extension.
/// </summary>
/// <typeparam name="TOptions">The options type being configured.</typeparam>
public class OptionsBuilder<TOptions> : Microsoft.Extensions.Options.OptionsBuilder<TOptions> where TOptions : class
{
    /// <summary/>
    public OptionsBuilder(IServiceCollection services, string name) : base(services, name)
    {
    }

    /// <summary>
    ///     Registers a configuration options source factory which <typeparamref name="TOptions"/> will bind against.
    /// </summary>
    public OptionsBuilder<TOptions> Bind(IConfigureOptionsSource<TOptions> source)
    {
        Services
            .AddSingleton<IOptionsChangeTokenSource<TOptions>>(_ => new LambdaOptionsChangeTokenSource<TOptions>(
                Name,
                source.GetChangeToken))
            .AddSingleton<IConfigureOptions<TOptions>>(_ => new ConfigureNamedOptions<TOptions>(
                Name,
                source.Configure));
        return this;
    }

    /// <summary>
    ///     Registers a configuration options source type which <typeparamref name="TOptions"/> will bind against.
    /// </summary>
    public OptionsBuilder<TOptions> Bind<TConfigureOptionsSource>()
    {
        var configureOptionsSourceType = typeof(IConfigureOptionsSource<TOptions>);

        if (!typeof(TConfigureOptionsSource).IsAssignableTo(configureOptionsSourceType))
            throw new ArgumentException($"Expected {configureOptionsSourceType.Name} implementation but received {typeof(TConfigureOptionsSource)}.");

        Services
            .TryAddSingleton(configureOptionsSourceType, configureOptionsSourceType)
            .AddSingleton<IOptionsChangeTokenSource<TOptions>>(p => new LambdaOptionsChangeTokenSource<TOptions>(
                Name,
                ((IConfigureOptionsSource<TOptions>)p.GetRequiredService(configureOptionsSourceType)).GetChangeToken))
            .AddSingleton<IConfigureOptions<TOptions>>(p => new ConfigureNamedOptions<TOptions>(
                Name,
                ((IConfigureOptionsSource<TOptions>)p.GetRequiredService(configureOptionsSourceType)).Configure));
        return this;
    }

    /// <summary>
    ///     Registers a cascade change dependency between <typeparamref name="TOptions"/> and <typrparamref name="TDependentOptions"/> options.
    /// </summary>
    public OptionsBuilder<TOptions> ChangeOn<TDependentOptions>(string name)
        where TDependentOptions : class
    {
        if (typeof(TOptions) == typeof(TDependentOptions))
            throw new ArgumentException("Dependent options cannot be equal to principal options.");

        var configureOptionsType = typeof(ChangeOnConfigureNamedOptions<,>).MakeGenericType(typeof(TOptions), typeof(TDependentOptions));

        Services.TryAddSingleton(p => (IConfigureOptions<TDependentOptions>)ActivatorUtilities.CreateInstance(
            p,
            configureOptionsType,
            Name,
            name));

        return this;
    }

    /// <summary>
    ///     Registers a cascade change dependency between <typeparamref name="TOptions"/> and <typrparamref name="TDependentOptions"/> options.
    /// </summary>
    public OptionsBuilder<TOptions> ChangeOn<TDependentOptions>()
        where TDependentOptions : class => ChangeOn<TDependentOptions>(Microsoft.Extensions.Options.Options.DefaultName);
}
