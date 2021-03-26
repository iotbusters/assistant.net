using System;

namespace Assistant.Net.Core
{
    public interface ISystemClock
    {
        DateTimeOffset UtcNow { get; }
    }
}