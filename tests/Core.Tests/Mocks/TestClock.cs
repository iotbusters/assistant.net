using Assistant.Net.Abstractions;
using System;

namespace Assistant.Net.Core.Tests.Mocks
{
    public class TestClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
    }
}
