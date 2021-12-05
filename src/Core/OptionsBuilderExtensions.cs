using Assistant.Net.Abstractions;
using Assistant.Net.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Assistant.Net
{
    /// <summary>
    ///     Options builder extensions for custom change configuration.
    /// </summary>
    public static class OptionsBuilderExtensions
    {
        /// <summary>
        ///     Registers a configuration options source factory which <typeparamref name="TOptions"/> will bind against.
        /// </summary>
        public static OptionsBuilder<TOptions> Bind<TOptions>(this OptionsBuilder<TOptions> builder, IConfigureOptionsSource<TOptions> source)
            where TOptions : class
        {
            builder.Services
                .AddSingleton<IOptionsChangeTokenSource<TOptions>>(_ => new NamedOptionsChangeTokenSource<TOptions>(
                    builder.Name,
                    source.GetChangeToken))
                .AddSingleton<IConfigureOptions<TOptions>>(_ => new ConfigureNamedOptions<TOptions>(
                    builder.Name,
                    source.Configure));
            return builder;
        }

        /// <summary>
        ///     Registers a configuration options source type which <typeparamref name="TOptions"/> will bind against.
        /// </summary>
        public static OptionsBuilder<TOptions> Bind<TOptions>(this OptionsBuilder<TOptions> builder, Type configureOptionsSourceType)
            where TOptions : class
        {
            if (!configureOptionsSourceType.IsAssignableTo(typeof(IConfigureOptionsSource<TOptions>)))
                throw new ArgumentException($"Expected {typeof(IConfigureOptionsSource<TOptions>).Name} implementation but received {configureOptionsSourceType}.");

            builder.Services
                .TryAddSingleton(configureOptionsSourceType, configureOptionsSourceType)
                .AddSingleton<IOptionsChangeTokenSource<TOptions>>(p => new NamedOptionsChangeTokenSource<TOptions>(
                    builder.Name,
                    ((IConfigureOptionsSource<TOptions>)p.GetRequiredService(configureOptionsSourceType)).GetChangeToken))
                .AddSingleton<IConfigureOptions<TOptions>>(p => new ConfigureNamedOptions<TOptions>(
                    builder.Name,
                    ((IConfigureOptionsSource<TOptions>)p.GetRequiredService(configureOptionsSourceType)).Configure));
            return builder;
        }

        /// <summary>
        ///     Registers a cascade change dependency between <typeparamref name="TOptions"/> <paramref name="dependentOptionsType"/> options.
        /// </summary>
        public static OptionsBuilder<TOptions> ChangeOn<TOptions>(this OptionsBuilder<TOptions> builder, string name, Type dependentOptionsType)
            where TOptions : class
        {
            if (typeof(TOptions) == dependentOptionsType)
                throw new ArgumentException("Dependent options cannot be equal to principal options.");

            var dependentPostConfigureOptionsInterfaceType = typeof(IPostConfigureOptions<>).MakeGenericType(dependentOptionsType);
            var dependentPostConfigureOptionsImplementationType = typeof(Internal.PostConfigureOptions<>).MakeGenericType(dependentOptionsType);

            builder.Services.AddSingleton(dependentPostConfigureOptionsInterfaceType, p =>
                Activator.CreateInstance(dependentPostConfigureOptionsImplementationType, name, new Action(() =>
                {
                    var cache = p.GetRequiredService<IOptionsMonitorCache<TOptions>>();
                    cache.TryRemove(builder.Name);
                }))!);
            return builder;
        }
    }
}
