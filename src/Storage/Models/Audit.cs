using System;
using System.Collections.Generic;
using static Assistant.Net.Storage.Internal.StoragePropertyNames;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     Associated value auditing details.
/// </summary>
public sealed class Audit
{
    /// <summary/>
    public Audit(IDictionary<string, string> details, long version)
    {
        Details = details;
        Version = version;
    }

    /// <summary/>
    public Audit(long version) : this(new Dictionary<string, string>(), version) { }

    /// <summary>
    ///     Value change related correlation id.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the value would be ignored if the detail is already set.
    /// </remarks>
    public string? CorrelationId
    {
        get => Details.GetOrDefault(CorrelationIdName);
        init => Details.TryAddUnlessDefault(CorrelationIdName, value);
    }

    /// <summary>
    ///     User created value.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the value would be ignored if the detail is already set.
    /// </remarks>
    public string? User
    {
        get => Details.GetOrDefault(UserName);
        init => Details.TryAddUnlessDefault(UserName, value);
    }

    /// <summary>
    ///     Value state version.
    /// </summary>
    public long Version { get; }

    /// <summary>
    ///     The date when value was created.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the value would be ignored if the detail is already set.
    /// </remarks>
    public DateTimeOffset Created
    {
        get => DateTimeOffset.Parse(Details.GetOrFail(CreatedName));
        init => Details.TryAddUnlessDefault(CreatedName, value.ToString("O"));
    }

    /// <summary>
    ///     All auditing details.
    /// </summary>
    public IDictionary<string, string> Details { get; }
}
