using System;

namespace Assistant.Net.Abstractions;

/// <summary>
///     System clock abstraction. Provides access to system data/time.
/// </summary>
public interface ISystemClock
{
    /// <summary>
    ///     Current UTC date/time.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}