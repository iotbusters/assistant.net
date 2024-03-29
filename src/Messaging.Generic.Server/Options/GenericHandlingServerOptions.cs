using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     Storage based server message handling configuration.
/// </summary>
public sealed class GenericHandlingServerOptions
{
    /// <summary>
    ///     List of registered messages for remote handling.
    /// </summary>
    /// <remarks>
    ///     It should be empty only if backoff handling was selected.
    /// </remarks>
    [MinLength(1)]
    public ISet<Type> MessageTypes { get; } = new HashSet<Type>();

    /// <summary>
    ///     Determine if message handling has backoff message handler configured.
    /// </summary>
    public bool HasBackoffHandler { get; set; }

    /// <summary>
    ///     Time to delay after no messages to handle were found.
    /// </summary>
    public TimeSpan InactivityDelayTime { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     Time to delay before next message handling attempt.
    /// </summary>
    public TimeSpan NextMessageDelayTime { get; set; } = TimeSpan.FromSeconds(0.01);
}
