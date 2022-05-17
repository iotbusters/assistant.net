using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests.Mocks;

public class TestMongoCursor<T> : IAsyncCursor<T>
{
    private readonly IEnumerable<T> items;

    public TestMongoCursor(params T[] items) =>
        this.items = items;

    public bool MoveNext(CancellationToken token)
    {
        if (Current != null!)
            return false;

        Current = items;
        return true;
    }

    public Task<bool> MoveNextAsync(CancellationToken token) =>
        Task.FromResult(MoveNext(token));

    public IEnumerable<T> Current { get; private set; } = null!;

    public void Dispose() { }
}
