using System;

namespace Assistant.Net.Messaging.Models;

/// <summary>
///     Remote message handler registration record.
/// </summary>
/// <param name="Instance">Remote server instance name.</param>
/// <param name="Messages">Remote server registered messages.</param>
/// <param name="AcceptOthers">Determine if remote server can accept other messages out of <paramref name="Messages"/>.</param>
/// <param name="Expired">The time when current record can be abandoned.</param>
public record HostRegistrationModel(string Instance, string[] Messages, bool AcceptOthers, DateTimeOffset Expired)
{
    /// <inheritdoc />
    public virtual bool Equals(HostRegistrationModel? other) => Instance == other?.Instance;

    /// <inheritdoc />
    public override int GetHashCode() => Instance.GetHashCode();
}
