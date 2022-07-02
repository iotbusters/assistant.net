using System.Collections.Generic;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     Historical storage detailed value.
/// </summary>
public class HistoricalValue<TValue> : StorageValue
{
    /// <summary/>
    public HistoricalValue(TValue value, IDictionary<string, string> details, long version) : base(details)
    {
        Value = value;
        Version = version;
    }

    /// <summary/>
    public HistoricalValue(TValue value, long version) : this(value, new Dictionary<string, string>(0), version) { }

    /// <summary>
    ///     Storage value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    ///     Storage value version.
    /// </summary>
    public long Version { get; }
}
