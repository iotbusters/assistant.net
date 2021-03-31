using System.Threading;
using Assistant.Net.Core.Abstractions;

namespace Assistant.Net.Core.Internal
{
    public class SystemLifetime : ISystemLifetime
    {
        private readonly CancellationTokenSource stopping;

        public SystemLifetime(CancellationToken stopping)
        {
            this.stopping = CancellationTokenSource.CreateLinkedTokenSource(stopping);
        }

        public CancellationToken Stopping => stopping.Token;
    }
}