using System.Threading;

namespace Assistant.Net.Core
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