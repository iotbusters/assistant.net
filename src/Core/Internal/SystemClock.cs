using System;
using Assistant.Net.Core.Abstractions;

namespace Assistant.Net.Core.Internal
{
    public class SystemClock : ISystemClock
    {
        private readonly Func<DateTimeOffset> getTime;

        public SystemClock(Func<DateTimeOffset> getTime)
        {
            this.getTime = getTime;
        }

        public DateTimeOffset UtcNow => getTime();
    }
}