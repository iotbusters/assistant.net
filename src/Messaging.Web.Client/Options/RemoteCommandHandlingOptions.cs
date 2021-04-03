using System;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Net.Messaging.Options
{
    public class RemoteCommandHandlingOptions
    {
        [Required]
        public Uri Endpoint { get; set; } = null!;

        public TimeSpan? Timeout { get; set; }
    }
}