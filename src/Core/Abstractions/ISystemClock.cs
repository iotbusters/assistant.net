using System;

namespace Assistant.Net.Abstractions
{
    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}