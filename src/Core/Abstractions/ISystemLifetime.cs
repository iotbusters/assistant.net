using System.Threading;

namespace Assistant.Net.Abstractions
{
    public interface ISystemLifetime
    {
        CancellationToken Stopping { get; }
    }
}