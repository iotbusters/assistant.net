using System;
using Assistant.Net.Abstractions;

namespace Assistant.Net.Internal
{
    /// <summary>
    ///     Default system clock implementation customized by a predicate.
    /// </summary>
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