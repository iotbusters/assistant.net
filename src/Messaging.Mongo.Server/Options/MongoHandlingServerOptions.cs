using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options;

/// <summary>
///     MongoDB server configuration used for remote message handling coordination.
/// </summary>
public class MongoHandlingServerOptions
{
    /// <summary>
    ///     Server instance ID.
    /// </summary>
    public int InstanceId { get; set; } = 1;

    /// <summary>
    ///     List of registered messages for remote handling.
    /// </summary>
    [MinLength(1)]
    public IList<Type> MessageTypes { get; } = new List<Type>();


    /// <summary>
    ///     Time to delay after no messages to handle were found.
    /// </summary>
    public TimeSpan InactivityDelayTime { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     Time to delay before next message handling attempt.
    /// </summary>
    public TimeSpan NextMessageDelayTime { get; set; } = TimeSpan.FromSeconds(0.01);
}
