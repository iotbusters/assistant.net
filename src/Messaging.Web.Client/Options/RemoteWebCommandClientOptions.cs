using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options
{
    /// <summary>
    ///     Remote command handling configuration.
    /// </summary>
    public class RemoteWebCommandClientOptions
    {
        /// <summary>
        ///     Remote WEB server URL.
        /// </summary>
        [Required]
        public Uri BaseAddress { get; set; } = null!;

        /// <summary>
        ///     Remote WEB request timeout.
        /// </summary>
        public TimeSpan? Timeout { get; set; } = null!;
    }
}