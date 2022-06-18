using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     WEB server configuration used for remote message handling coordination.
/// </summary>
public sealed class WebHandlingServerOptions
{
    /// <summary>
    ///     List of accepting message types.
    /// </summary>
    [MinLength(1)]
    public List<Type> MessageTypes { get; } = new();
}
