using System;

namespace Assistant.Net.Messaging.Models;

/// <summary>
///     Remote message handler registration record.
/// </summary>
/// <param name="Instance">Remote server instance name.</param>
/// <param name="Expired">The time when current record can be abandoned.</param>
public record RemoteHandlerRegistration(string Instance, DateTimeOffset Expired);
