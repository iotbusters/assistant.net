using System;
using NUnit.Framework;

namespace Assistant.Net.Serialization.Json.Tests.Mocks;

public class TestObjectWithPrivateCtor
{
    public TestObjectWithPrivateCtor()
    {
        Assert.Fail("Invalid ctor was selected.");
    }

    public TestObjectWithPrivateCtor(
        TestEnum? @enum,
        string? @string,
        int? integerNumber)
    {
        Assert.Fail("Invalid ctor was selected.");
    }

    private TestObjectWithPrivateCtor(
        TestEnum? @enum,
        string? @string,
        int? integerNumber,
        float? floatNumber,
        decimal? decimalNumber,
        DateTime? dateTime)
    {
        Enum = @enum;
        String = @string;
        IntegerNumber = integerNumber;
        FloatNumber = floatNumber;
        DecimalNumber = decimalNumber;
        DateTime = dateTime;
    }

    public TestEnum? Enum { get; }
    public string? String { get; }
    public int? IntegerNumber { get; }
    public float? FloatNumber { get; }
    public decimal? DecimalNumber { get; }
    public DateTime? DateTime { get; }

    public static TestObjectWithPrivateCtor New(
        TestEnum? @enum,
        string? @string,
        int? integerNumber,
        float? floatNumber,
        decimal? decimalNumber,
        DateTime? dateTime) =>
        new(TestEnum.A, "A", 12, 12f, 12m, System.DateTime.UtcNow);
}
