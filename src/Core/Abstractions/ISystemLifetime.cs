using System.Threading;

namespace Assistant.Net.Core.Abstractions
{
    public interface ISystemLifetime
    {
        CancellationToken Stopping { get; }
    }
}