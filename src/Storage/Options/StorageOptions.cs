using Assistant.Net.Options;
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
    public Dictionary<Type, InstanceFactory<object>> DefaultConverters { get; } = new();

    /// <summary>
    ///     Specific type regular storage provider factories.
    /// </summary>
    public Dictionary<Type, InstanceFactory<object>> Providers { get; } = new();

    /// <summary>
    ///     Specific type historical storage provider factories.
    /// </summary>
    public Dictionary<Type, InstanceFactory<object>> HistoricalProviders { get; } = new();

    /// <summary>
    ///     Specific type partitioned storage provider factories.
    /// </summary>
    public Dictionary<Type, InstanceFactory<object>> PartitionedProviders { get; } = new();

    /// <summary>
    ///     Any other type regular storage provider factories except defined in <see cref="Providers"/>.
    /// </summary>
    public InstanceFactory<object, Type>? AnyProvider { get; internal set; }

    /// <summary>
    ///     Any other type historical storage provider factories except defined in <see cref="HistoricalProviders"/>.
    /// </summary>
    public InstanceFactory<object, Type>? AnyHistoricalProvider { get; internal set; }

    /// <summary>
    ///     Any other type partitioned storage provider factories except defined in <see cref="PartitionedProviders"/>.
    /// </summary>
    public InstanceFactory<object, Type>? AnyPartitionedProvider { get; internal set; }

    /// <summary>
    ///     Single regular storage provider factory.
    /// </summary>
    public InstanceFactory<object, Type>? SingleProvider { get; internal set; }

    /// <summary>
    ///     Single historical storage provider factory.
    /// </summary>
    public InstanceFactory<object, Type>? SingleHistoricalProvider { get; internal set; }

    /// <summary>
    ///     Single partitioned storage provider factory.
    /// </summary>
    public InstanceFactory<object, Type>? SinglePartitionedProvider { get; internal set; }
}
