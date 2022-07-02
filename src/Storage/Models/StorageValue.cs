﻿using System.Collections.Generic;
using static Assistant.Net.Storage.Internal.StoragePropertyNames;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     Regular storage detailed value.
/// </summary>
public sealed class StorageValue<TValue> : StorageValue
{
    /// <summary/>
    public StorageValue(TValue value) : this(value, new Dictionary<string, string>(0)) { }

    /// <summary/>
    public StorageValue(TValue value, IDictionary<string, string> details) : base(details) =>
        Value = value;

    /// <summary>
    ///     Storage value.
    /// </summary>
    public TValue Value { get; }
}

/// <summary>
///     Base class of detailed storage value.
/// </summary>
public abstract class StorageValue
{
    /// <summary/>
    protected StorageValue(IDictionary<string,string> details) =>
        Details = details;

    /// <summary>
    ///     Storage value details.
    /// </summary>
    public IDictionary<string, string> Details { get; }

    /// <summary>
    ///     Storage value details indexer.
    /// </summary>
    public string? this[string propertyName]
    {
        get => Details.GetOrDefault(propertyName);
        set => Details.SetUnlessDefault(propertyName, value);
    }

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
}
