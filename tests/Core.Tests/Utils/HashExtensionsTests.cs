﻿using Assistant.Net.Core.Tests.Mocks;
using Assistant.Net.Utils;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace Assistant.Net.Core.Tests.Utils;

public class HashExtensionsTests
{
    [TestCaseSource(nameof(GetHashGenerationCases))]
    public void GetSha1_returnsHashCode(object value, string hashCode) =>
        value.GetSha1().Should().Be(hashCode);

    [Test]
    public void GetSha1_throws_null()
    {
        this.Invoking(_ => ((int?)null).GetSha1()).Should().Throw<ArgumentNullException>();
        this.Invoking(_ => ((string?)null!).GetSha1()).Should().Throw<ArgumentNullException>();
        this.Invoking(_ => ((byte[]?)null!).GetSha1()).Should().Throw<ArgumentNullException>();
        this.Invoking(_ => ((Stream?)null!).GetSha1()).Should().Throw<ArgumentNullException>();
    }

    [TestCaseSource(nameof(GetValues))]
    public void GetSha1_returnsReproducibleHash(object value) =>
        value.GetSha1().Should().Be(value.GetSha1());

    [TestCaseSource(nameof(GetHashGenerationUnequalCases))]
    public void GetSha1_returnsUnequalHashes(object value1, object value2) =>
        value1.GetSha1().Should().NotBe(value2.GetSha1());

    public static IEnumerable<object> GetValues() =>
        GetHashGenerationCases().Select(x => x.Arguments[0]!).Where(x => x is not Stream);

    public static IEnumerable<object> GetHashGenerationUnequalCases()
    {
        yield return new TestCaseData("", "1");
        yield return new TestCaseData("1", "2");
        yield return new TestCaseData(1, 2);
        yield return new TestCaseData(1, 1L);
        yield return new TestCaseData(1, 1f);
        yield return new TestCaseData(1, 1d);
        yield return new TestCaseData(1, 1m);
        yield return new TestCaseData(1d, 1m);
        yield return new TestCaseData(1d, 1f);
        yield return new TestCaseData(1.1f, 2.1f);
        yield return new TestCaseData(1.1d, 2.1d);
        yield return new TestCaseData(1.1m, 2.1m);
        yield return new TestCaseData(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        yield return new TestCaseData(new DateTime(1111, 1, 1, 1, 1, 1), new DateTime(1111, 1, 1, 1, 1, 2));
        yield return new TestCaseData(new DateTime(1111, 1, 1, 1, 1, 1, DateTimeKind.Utc), new DateTimeOffset(1111, 1, 1, 1, 1, 1, new(0, 1, 0, 0)));
        yield return new TestCaseData(new DateTimeOffset(1111, 1, 1, 1, 1, 1, new(0, 1, 0, 0)), new DateTimeOffset(1111, 1, 1, 1, 1, 1, new(0, 2, 0, 0)));
        yield return new TestCaseData(new[] {1, 2}, new[] {1, 2, 3});
        yield return new TestCaseData(new[] {1, 2, 3}, new[] {3, 2, 1});
        yield return new TestCaseData(new TestStruct("123"), new TestStruct("321"));
        yield return new TestCaseData(new TestFixedStruct(TimeSpan.FromSeconds(1)), new TestFixedStruct(TimeSpan.FromSeconds(2)));
        yield return new TestCaseData(TestEnum.One, TestEnum.Two);
    }

    public static IEnumerable<TestCaseData> GetHashGenerationCases()
    {
        yield return new("", "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(Encoding.UTF8.GetBytes("123"), "QL0AFWMIX8NRZTKeof9cXsvbvu8=");
        yield return new(new[] {1, 2, 3}, "5CnMo/cDo5zFlUplcv7JCGE1s04=");
        yield return new(Array.Empty<byte>(), "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(Array.Empty<int>(), "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(Array.Empty<byte>().ToList(), "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(Array.Empty<byte>().ToImmutableArray(), "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(Array.Empty<byte>().ToImmutableList(), "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(Array.Empty<int>().ToList(), "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(Array.Empty<Guid>().ToImmutableArray(), "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(Array.Empty<Guid>().ToImmutableList(), "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(new MemoryStream(), "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new(new MemoryStream(Encoding.UTF8.GetBytes("123")), "QL0AFWMIX8NRZTKeof9cXsvbvu8=");
        yield return new("123", "QL0AFWMIX8NRZTKeof9cXsvbvu8=");
        yield return new(123, "GVmJP2giBFnL2AA5bh6ue/w4Lpc=");
        yield return new(123L, "bdYNn8pZRfNzUKFV3N4reh2uX2Y=");
        yield return new(123f, "wZfwyOyfHytX8daUfciYnPtBjPs=");
        yield return new(123d, "ngna5ErtpwZPDQJlwV2O+kauna4=");
        yield return new(true, "PFhWBOh/hVlzcx/qg+Ifq5OS0vw=");
        yield return new((byte)1, "v4tFMNjSRt10rFOhNHG7oXlB3/c=");
        yield return new(new Guid("8f575d06-4d90-4837-8934-be5362a94e40"), "mzJtM1MQiol2quW4QUnc2yFrspA=");
        yield return new(new DateTime(1111, 1, 1, 1, 1, 1, DateTimeKind.Utc), "vZFSjTZXg0kd4d4I1jCLwaRmA7o=");
        yield return new(new DateTimeOffset(1111, 1, 1, 1, 1, 1, new(0, 0, 0, 0)), "vZFSjTZXg0kd4d4I1jCLwaRmA7o=");
        yield return new(new DateTimeOffset(1111, 1, 1, 1, 1, 1, new(0, 1, 0, 0)), "uULqXZzBVkjrk6iD8mVCRB0+fnQ=");
        yield return new(new TimeSpan(1, 1, 1, 1), "9pEtKDY4oxlX6XNesJn9hA9nU/Q=");
        var value = new TestClass {Value1 = "value", Value2 = 123, Value3 = null, Value4 = new[] {1.2, 2.3}, Value5 = new TestClass()};
        yield return new(value, "UPasmD4uUAxbKUvHlUsJ672Ij+4=");
        yield return new(new TestStruct("123"), "Kkw2SQrZwH3mbHcH/M55oMFE2gk=");
        yield return new(new TestFixedStruct(TimeSpan.FromSeconds(1)), "wt/4kjh8kSmuGGTeV4lv6p7CYhs=");
        yield return new(TestEnum.One, "tYtajO2dtIsw4AixSABMEGXOU7E=");
    }
}
