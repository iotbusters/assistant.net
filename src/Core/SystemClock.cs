using System;

namespace Assistant.Net.Core
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