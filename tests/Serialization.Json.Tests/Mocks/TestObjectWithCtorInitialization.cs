using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Assistant.Net.Serialization.Json.Tests.Mocks;

public class TestObjectWithCtorInitialization
{
    public TestObjectWithCtorInitialization()
    {
        Assert.Fail("Invalid ctor was selected.");
    }

    public TestObjectWithCtorInitialization(
        int?[]? integerArray,
        IEnumerable<string?>? stringArray,
        TestClass?[]? objectArray)
    {
        Assert.Fail("Invalid ctor was selected.");
    }

    public TestObjectWithCtorInitialization(
        TestEnum? @enum,
        string? @string,
        int? integerNumber,
        float? floatNumber,
        decimal? decimalNumber,
        DateTime? dateTime,
        int?[]? integerArray,
        IEnumerable<string?>? stringArray,
        TestClass?[]? objectArray)
    {
        Enum = @enum;
        String = @string;
        IntegerNumber = integerNumber;
        FloatNumber = floatNumber;
        DecimalNumber = decimalNumber;
        DateTime = dateTime;
        IntegerArray = integerArray;
        StringArray = stringArray;
        ObjectArray = objectArray;
    }

    public TestEnum? Enum { get; }
    public string? String { get; }
    public int? IntegerNumber { get; }
    public float? FloatNumber { get; }
    public decimal? DecimalNumber { get; }
    public DateTime? DateTime { get; }
    public int?[]? IntegerArray { get; }
    public IEnumerable<string?>? StringArray { get; }
    public TestClass?[]? ObjectArray { get; }
}
