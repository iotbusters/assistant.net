using System;
using System.Collections.Generic;

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
    public Audit(string? correlationId, string? user, DateTimeOffset created, long version)
        : this(new Dictionary<string, string>(), version)
    {
        CorrelationId = correlationId;
        User = user;
        Created = created;
    }

    /// <summary>
    ///     Value change related correlation id.
    /// </summary>
    public string? CorrelationId
    {
        get => GetString("correlationId");
        set => SetString("correlationId", value);
    }

    /// <summary>
    ///     User created value.
    /// </summary>
    public string? User
    {
        get => GetString("user");
        set => SetString("user", value);
    }

    /// <summary>
    ///     Value state version.
    /// </summary>
    public long Version { get; }

    /// <summary>
    ///     The date when value was created.
    /// </summary>
    public DateTimeOffset Created
    {
        get => GetDate("created");
        set => SetDate("created", value);
    }

    /// <summary>
    ///     All auditing details.
    /// </summary>
    public IDictionary<string, string> Details { get; set; }

    private void SetString(string propertyName, string? propertyValue)
    {
        if (propertyValue != null)
            Details[propertyName] = propertyValue;
    }

    private void SetDate(string propertyName, DateTimeOffset? propertyValue) =>
        Details[propertyName] = propertyValue?.ToString("O") ?? throw new ArgumentNullException(nameof(propertyValue));

    private string? GetString(string name) =>
        Details.TryGetValue(name, out var value) ? value : null;

    private DateTimeOffset GetDate(string name) =>
        Details.TryGetValue(name, out var value) ? DateTimeOffset.Parse(value) : throw new NotImplementedException();
}
