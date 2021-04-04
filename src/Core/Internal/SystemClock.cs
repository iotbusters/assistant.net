using System;
using Assistant.Net.Abstractions;

namespace Assistant.Net.Internal
{
    internal class SystemClock : ISystemClock
    {
        private readonly Func<DateTimeOffset> getTime;

        public SystemClock(Func<DateTimeOffset> getTime)
        {
            this.getTime = getTime;
        }

        public DateTimeOffset UtcNow => getTime();
    }
}