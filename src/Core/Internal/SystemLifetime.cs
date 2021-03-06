using System.Threading;
using Assistant.Net.Abstractions;

namespace Assistant.Net.Internal
{
    /// <summary>
    ///     Default system lifetime implementation.
    /// </summary>
    internal class SystemLifetime : ISystemLifetime
    {
        private readonly CancellationTokenSource stopping;

        public SystemLifetime(CancellationToken stopping)
        {
            this.stopping = CancellationTokenSource.CreateLinkedTokenSource(stopping);
        }

        public CancellationToken Stopping => stopping.Token;
    }
}