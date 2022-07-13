namespace Assistant.Net.Storage.Sqlite.Tests.Mocks;

public record TestKey2(int Value1, string Value2)
{
    public TestKey2(int value1) : this(value1, value1.ToString()) { }
}
