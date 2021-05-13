using System.Threading;

namespace Assistant.Net.Abstractions
{
    /// <summary>
    ///     System lifetime abstraction. Provides access to application lifetime events.
    /// </summary>
    public interface ISystemLifetime
    {
        /// <summary>
        ///     System is stopping event.
        /// </summary>
        CancellationToken Stopping { get; }
    }
}