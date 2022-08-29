using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace Assistant.Net.Logging.Tests.Mocks;

public sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
{
    private readonly T options;

    public TestOptionsMonitor(T options) => this.options = options;

    public T Get(string name) => options;

    public IDisposable OnChange(Action<T, string> listener) => new StringReader(string.Empty);

    public T CurrentValue => options;
}
