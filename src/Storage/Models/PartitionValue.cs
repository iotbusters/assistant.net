using System.Collections.Generic;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     Partitioned storage detailed value.
/// </summary>
public class PartitionValue<TValue> : StorageValue
{
    /// <summary/>
    public PartitionValue(TValue value, IDictionary<string, string> details, long index) : base(details)
    {
        Value = value;
        Index = index;
    }

    /// <summary/>
    public PartitionValue(TValue value, long version) : this(value, new Dictionary<string, string>(0), version) { }

    /// <summary>
    ///     Storage value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    ///     Storage value partition index.
    /// </summary>
    public long Index { get; }
}
