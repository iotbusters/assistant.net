using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     WEB based message handling client configuration.
/// </summary>
public class WebHandlerProxyOptions
{
    /// <summary>
    ///     Associated <see cref="HttpClient"/> configurations.
    /// </summary>
    public List<Action<HttpClient>> Configurations { get; } = new();
}
