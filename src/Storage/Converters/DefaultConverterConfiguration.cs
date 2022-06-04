using Assistant.Net.Options;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Assistant.Net.Storage.Converters;

/// <summary>
///     Configuration with default set of <see cref="IValueConverter{TValue}"/>s for primitive types.
/// </summary>
public class DefaultConverterConfiguration : IStorageConfiguration
{
    /// <inheritdoc/>
    public void Configure(StorageBuilder builder)
    {
        builder.Services
            .TryAddSingleton<PrimitiveValueConverter>()
            .ConfigureStorageOptions(builder.Name, o =>
            {
                var factory = new InstanceFactory<object>(p => p.GetRequiredService<PrimitiveValueConverter>());
                o.DefaultConverters[typeof(string)] = factory;
                o.DefaultConverters[typeof(Guid)] = factory;
                o.DefaultConverters[typeof(bool)] = factory;
                o.DefaultConverters[typeof(int)] = factory;
                o.DefaultConverters[typeof(float)] = factory;
                o.DefaultConverters[typeof(double)] = factory;
                o.DefaultConverters[typeof(decimal)] = factory;
                o.DefaultConverters[typeof(string)] = factory;
                o.DefaultConverters[typeof(TimeSpan)] = factory;
                o.DefaultConverters[typeof(DateTime)] = factory;
                o.DefaultConverters[typeof(DateTimeOffset)] = factory;
            });
    }
}
