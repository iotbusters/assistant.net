using System.Collections.Generic;

namespace Assistant.Net.Core.Tests.Mocks;

public struct TestStruct
{
    public TestStruct() : this("123") { }

    public TestStruct(IEnumerable<char> @string) =>
        String = @string;

    public IEnumerable<char> String { get; set; }
}
