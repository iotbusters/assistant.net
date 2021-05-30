using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Remote command handling configuration.
    /// </summary>
    public class RemoteWebCommandClientOptions
    {
        [Required]
        public Uri BaseAddress { get; set; } = null!;

        public TimeSpan? Timeout { get; set; }
    }
}