using System;

namespace Assistant.Net.Core.Tests.Mocks;

public struct TestFixedStruct
{
    public TestFixedStruct(TimeSpan t) =>
        T = t;

    private readonly TimeSpan T;
    public char C = '1';
    public int I = 1;
}
