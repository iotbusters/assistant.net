using Assistant.Net.Storage.Options;

namespace Assistant.Net.Storage.Abstractions;

/// <summary>
///     Common storage configuration abstraction required for grouping configurations by purpose and
///     resolving code duplication issues and improving code readability.
/// </summary>
public interface IStorageConfiguration
{
    /// <summary>
    ///     Configures storages.
    /// </summary>
    void Configure(StorageBuilder builder);
}
