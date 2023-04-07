using Assistant.Net.Options;
using Assistant.Net.Storage.Abstractions;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Storage.Options;

/// <summary>
///     Storage configuration for specific provider usage.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>
    ///     Default type value converter factories.
    /// </summary>
    public Dictionary<Type, InstanceFactory<object>> Converters { get; } = new();

    /// <summary>
    ///     Specific storing type registrations.
    /// </summary>
    public HashSet<Type> Registrations { get; } = new();

    /// <summary>
    ///     Determine if any type is allowed despite configured <see cref="Registrations"/>.
    /// </summary>
    public bool IsAnyTypeAllowed { get; internal set; }

    /// <summary>
    ///     Specific type regular storage provider factory.
    /// </summary>
    public InstanceFactory<IStorageProvider, Type>? StorageProviderFactory { get; internal set; } = null!;

    /// <summary>
    ///     Specific type historical storage provider factory.
    /// </summary>
    public InstanceFactory<IHistoricalStorageProvider, Type>? HistoricalStorageProviderFactory { get; internal set; } = null!;

    /// <summary>
    ///     Specific type partitioned storage provider factory.
    /// </summary>
    public InstanceFactory<IPartitionedStorageProvider, Type>? PartitionedStorageProviderFactory { get; internal set; } = null!;
}
