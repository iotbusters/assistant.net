using System.Threading;

namespace Assistant.Net.Core
{
    public interface ISystemLifetime
    {
        CancellationToken Stopping { get; }
    }
}