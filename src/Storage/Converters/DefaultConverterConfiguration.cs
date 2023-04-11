using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Options;

namespace Assistant.Net.Storage.Converters;

/// <summary>
///     Configuration with default set of <see cref="IValueConverter{TValue}"/>s for primitive types.
/// </summary>
public class DefaultConverterConfiguration : IStorageConfiguration
{
    /// <inheritdoc/>
    public void Configure(StorageBuilder builder)
    {
        builder.Services.TryAddSingleton<PrimitiveValueConverter>();
        builder.AddConverter<PrimitiveValueConverter>();
    }
}
