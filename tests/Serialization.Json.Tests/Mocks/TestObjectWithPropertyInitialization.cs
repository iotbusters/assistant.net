using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Assistant.Net.Serialization.Json.Tests.Mocks;

public class TestObjectWithPropertyInitialization
{
    public TestObjectWithPropertyInitialization() { }

    public TestObjectWithPropertyInitialization(string unknown)
    {
        Assert.Fail("Invalid ctor was selected.");
    }

    public TestObjectWithPropertyInitialization(
        string? unknown,
        TestEnum? @enum,
        string? @string,
        int? integerNumber,
        float? floatNumber,
        decimal? decimalNumber,
        DateTime? dateTime,
        int?[]? integerArray,
        IEnumerable<string>? stringArray,
        TestClass[]? objectArray)
    {
        Assert.Fail("Invalid ctor was selected.");
    }

    public TestEnum? Enum { get; set; } = null!;
    public string? String { get; set; } = null!;
    public int? IntegerNumber { get; set; } = null!;
    public float? FloatNumber { get; set; } = null!;
    public decimal? DecimalNumber { get; set; } = null!;
    public DateTime? DateTime { get; set; } = null!;
    public int?[]? IntegerArray { get; set; } = null!;
    public IEnumerable<string?>? StringArray { get; set; } = null!;
    public TestClass?[]? ObjectArray { get; set; } = null!;
}
