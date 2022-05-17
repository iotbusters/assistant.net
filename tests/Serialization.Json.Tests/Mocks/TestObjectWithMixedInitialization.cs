using NUnit.Framework;

namespace Assistant.Net.Serialization.Json.Tests.Mocks;

public class TestObjectWithMixedInitialization
{
    public TestObjectWithMixedInitialization()
    {
        Assert.Fail("Invalid ctor was selected.");
    }

    public TestObjectWithMixedInitialization(TestEnum? unknown1, string @string)
    {
        Assert.Fail("Invalid ctor was selected.");
    }

    public TestObjectWithMixedInitialization(
        string? unknown,
        TestEnum? @enum,
        string? @string)
    {
        Assert.Fail("Invalid ctor was selected.");
    }

    public TestObjectWithMixedInitialization(TestEnum? @enum)
    {
        Enum = @enum;
    }

    public TestEnum? Enum { get; set; } = null!;
    public string? String { get; set; } = null!;
    public int? NullableNumber { get; set; } = 10;
}
