using Assistant.Net.Options;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Storage.Options;

/// <summary>
///     Storage configuration for specific provider usage.
/// </summary>
public class StorageOptions
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
    ///     Any other type (except defined in <see cref="Providers"/>) regular storage provider factories.
    /// </summary>
    public InstanceFactory<object, Type>? ProviderAny { get; set; }

    /// <summary>
    ///     Any other type (except defined in <see cref="HistoricalProviders"/>) historical storage provider factories.
    /// </summary>
    public InstanceFactory<object, Type>? HistoricalProviderAny { get; set; }

    /// <summary>
    ///     Any other type (except defined in <see cref="PartitionedProviders"/>) partitioned storage provider factories.
    /// </summary>
    public InstanceFactory<object, Type>? PartitionedProviderAny { get; set; }

    /// <summary>
    ///     Single regular storage provider factory.
    /// </summary>
    public InstanceFactory<object, Type>? SingleProvider { get; set; }

    /// <summary>
    ///     Single historical storage provider factory.
    /// </summary>
    public InstanceFactory<object, Type>? SingleHistoricalProvider { get; set; }

    /// <summary>
    ///     Single partitioned storage provider factory.
    /// </summary>
    public InstanceFactory<object, Type>? SinglePartitionedProvider { get; set; }
}
