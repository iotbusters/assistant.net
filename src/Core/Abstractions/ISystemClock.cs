using System;

namespace Assistant.Net.Core.Abstractions
{
    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}