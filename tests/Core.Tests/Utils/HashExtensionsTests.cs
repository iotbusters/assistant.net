using Assistant.Net.Core.Tests.Mocks;
using Assistant.Net.Utils;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Assistant.Net.Core.Tests.Utils;

public class HashExtensionsTests
{
    [TestCaseSource(nameof(GetSha1GenerationCases))]
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

    [TestCaseSource(nameof(GetSha1Values))]
    public void GetSha1_returnsReproducibleHash(object value) =>
        value.GetSha1().Should().Be(value.GetSha1());

    [TestCaseSource(nameof(GetHashGenerationUnequalCases))]
    public void GetSha1_returnsUnequalHashes(object value1, object value2) =>
        value1.GetSha1().Should().NotBe(value2.GetSha1());

    [TestCaseSource(nameof(GetSha256GenerationCases))]
    public void GetSha256_returnsHashCode(object value, string hashCode) =>
        value.GetSha256().Should().Be(hashCode);

    [Test]
    public void GetSha256_throws_null()
    {
        this.Invoking(_ => ((int?)null).GetSha256()).Should().Throw<ArgumentNullException>();
        this.Invoking(_ => ((string?)null!).GetSha256()).Should().Throw<ArgumentNullException>();
        this.Invoking(_ => ((byte[]?)null!).GetSha256()).Should().Throw<ArgumentNullException>();
        this.Invoking(_ => ((Stream?)null!).GetSha256()).Should().Throw<ArgumentNullException>();
    }

    [TestCaseSource(nameof(GetSha256Values))]
    public void GetSha256_returnsReproducibleHash(object value) =>
        value.GetSha256().Should().Be(value.GetSha256());

    [TestCaseSource(nameof(GetHashGenerationUnequalCases))]
    public void GetSha256_returnsUnequalHashes(object value1, object value2) =>
        value1.GetSha256().Should().NotBe(value2.GetSha256());

    [TestCaseSource(nameof(GetMd5GenerationCases))]
    public void GetMd5_returnsHashCode(object value, string hashCode) =>
        value.GetMd5().Should().Be(hashCode);

    [Test]
    public void GetMd5_throws_null()
    {
        this.Invoking(_ => ((int?)null).GetMd5()).Should().Throw<ArgumentNullException>();
        this.Invoking(_ => ((string?)null!).GetMd5()).Should().Throw<ArgumentNullException>();
        this.Invoking(_ => ((byte[]?)null!).GetMd5()).Should().Throw<ArgumentNullException>();
        this.Invoking(_ => ((Stream?)null!).GetMd5()).Should().Throw<ArgumentNullException>();
    }

    [TestCaseSource(nameof(GetMd5Values))]
    public void GetMd5_returnsReproducibleHash(object value) =>
        value.GetMd5().Should().Be(value.GetMd5());

    [TestCaseSource(nameof(GetHashGenerationUnequalCases))]
    public void GetMd5_returnsUnequalHashes(object value1, object value2) =>
        value1.GetMd5().Should().NotBe(value2.GetMd5());

    public static IEnumerable<object> GetSha1Values() =>
        GetSha1GenerationCases().Select(x => x.Arguments[0]!).Where(x => x is not Stream);

    public static IEnumerable<object> GetSha256Values() =>
        GetSha256GenerationCases().Select(x => x.Arguments[0]!).Where(x => x is not Stream);

    public static IEnumerable<object> GetMd5Values() =>
        GetMd5GenerationCases().Select(x => x.Arguments[0]!).Where(x => x is not Stream);

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

    public static IEnumerable<TestCaseData> GetSha1GenerationCases()
    {
        yield return new("", "2jmj7l5rSw0yVb/vlWAYkK/YBwk=");
        yield return new("123"u8.ToArray(), "QL0AFWMIX8NRZTKeof9cXsvbvu8=");
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
        yield return new(new MemoryStream("123"u8.ToArray()), "QL0AFWMIX8NRZTKeof9cXsvbvu8=");
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

    public static IEnumerable<TestCaseData> GetSha256GenerationCases()
    {
        yield return new("", "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new("123"u8.ToArray(), "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=");
        yield return new(new[] {1, 2, 3}, "RjaZPT4dpOnWuPh7eej3xtAYWA1SZhlQ6rw4RcWJek0=");
        yield return new(Array.Empty<byte>(), "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new(Array.Empty<int>(), "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new(Array.Empty<byte>().ToList(), "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new(Array.Empty<byte>().ToImmutableArray(), "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new(Array.Empty<byte>().ToImmutableList(), "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new(Array.Empty<int>().ToList(), "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new(Array.Empty<Guid>().ToImmutableArray(), "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new(Array.Empty<Guid>().ToImmutableList(), "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new(new MemoryStream(), "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=");
        yield return new(new MemoryStream("123"u8.ToArray()), "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=");
        yield return new("123", "pmWkWSBCL51Bfkhn79xPuKBKHz//H6B+mY6G9/eieuM=");
        yield return new(123, "pdz1uEGN+v7BYHkUjskM+B38YnbBzOIgAXx4Lst9euo=");
        yield return new(123L, "TzGZh6eGEH3GOytwEVs3NMuYgLCZtwxGPF4bBVIat2Q=");
        yield return new(123f, "MkJkNMESn6rSTXYt2fdTaJ5YVV8GA+6q4gB3OyP9nwg=");
        yield return new(123d, "XA5MSP8a13kG6PiG9hLHobk9hmD62dGFrDqZdj7WVmc=");
        yield return new(true, "Z6vdchAk8P9OCz9ML8E7xbrULQt4UdRW2I0gPRWqpFA=");
        yield return new((byte)1, "S/USLzRFVMU73i67jNK349FgCtYxw4Wl18ziPHeFRZo=");
        yield return new(new Guid("8f575d06-4d90-4837-8934-be5362a94e40"), "iJgW5fKvJlqmfXM596DCST8rUnvlJlIuRf4BU022MSM=");
        yield return new(new DateTime(1111, 1, 1, 1, 1, 1, DateTimeKind.Utc), "B128+xyBWnxz9ouPL7G7R+VPlBhtq0G3Qd3StYWMzKs=");
        yield return new(new DateTimeOffset(1111, 1, 1, 1, 1, 1, new(0, 0, 0, 0)), "B128+xyBWnxz9ouPL7G7R+VPlBhtq0G3Qd3StYWMzKs=");
        yield return new(new DateTimeOffset(1111, 1, 1, 1, 1, 1, new(0, 1, 0, 0)), "v1bol2CRimQi3JTTPOCO+A5od8FEXj5VcnaA1ojwOS0=");
        yield return new(new TimeSpan(1, 1, 1, 1), "muG8kLAp9ZkoTuz+wPG3IrnS443Q/AdIIzeO93E1jro=");
        var value = new TestClass {Value1 = "value", Value2 = 123, Value3 = null, Value4 = new[] {1.2, 2.3}, Value5 = new TestClass()};
        yield return new(value, "TPcYqkYYSahRyFGnLDaC3DNySkcDkn3qSXUQ9WbKyYw=");
        yield return new(new TestStruct("123"), "Za9ALY0Ojl05amq1RbHstc/qY+o21pLCuqbk0+ZU6e0=");
        yield return new(new TestFixedStruct(TimeSpan.FromSeconds(1)), "4AFjQfRSjSE+bchhHA3WOSFuaXk7eh/MDFH6pcTI+gs=");
        yield return new(TestEnum.One, "ixJQd4PVvsrL8uvlsBpgAk2HKKj4bcyBi85pnoszILw=");
    }

    public static IEnumerable<TestCaseData> GetMd5GenerationCases()
    {
        yield return new("", "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new("123"u8.ToArray(), "ICy5YqxZB1uWSwcVLSNLcA==");
        yield return new(new[] {1, 2, 3}, "Kh3R4eWdCjhMJpUeMWzX5g==");
        yield return new(Array.Empty<byte>(), "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new(Array.Empty<int>(), "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new(Array.Empty<byte>().ToList(), "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new(Array.Empty<byte>().ToImmutableArray(), "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new(Array.Empty<byte>().ToImmutableList(), "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new(Array.Empty<int>().ToList(), "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new(Array.Empty<Guid>().ToImmutableArray(), "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new(Array.Empty<Guid>().ToImmutableList(), "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new(new MemoryStream(), "1B2M2Y8AsgTpgAmY7PhCfg==");
        yield return new(new MemoryStream("123"u8.ToArray()), "ICy5YqxZB1uWSwcVLSNLcA==");
        yield return new("123", "ICy5YqxZB1uWSwcVLSNLcA==");
        yield return new(123, "0Rn6vgOLxdBJYFFlj9IF5g==");
        yield return new(123L, "8YuNvv4CoO/OKB3rVaIJzQ==");
        yield return new(123f, "35NHiNVJi7zwsxAB26pVLQ==");
        yield return new(123d, "WxSlc8Ty396zRNXS+Tf9zA==");
        yield return new(true, "Q1LYiniqOXUL9wzW8nvKpQ==");
        yield return new((byte)1, "VaVACK0bpYmqIQ0mKcHfQQ==");
        yield return new(new Guid("8f575d06-4d90-4837-8934-be5362a94e40"), "zgpZJ4OompnrXKb3hu16XQ==");
        yield return new(new DateTime(1111, 1, 1, 1, 1, 1, DateTimeKind.Utc), "8GJL9ZvxXQZAOj/o9LDcCA==");
        yield return new(new DateTimeOffset(1111, 1, 1, 1, 1, 1, new(0, 0, 0, 0)), "8GJL9ZvxXQZAOj/o9LDcCA==");
        yield return new(new DateTimeOffset(1111, 1, 1, 1, 1, 1, new(0, 1, 0, 0)), "h6iHg6MCTrnj+JIdBjct8g==");
        yield return new(new TimeSpan(1, 1, 1, 1), "MoVJx4mSwvIy+Eal9JS7Fw==");
        var value = new TestClass {Value1 = "value", Value2 = 123, Value3 = null, Value4 = new[] {1.2, 2.3}, Value5 = new TestClass()};
        yield return new(value, "qXy54wBcSvYdb8kOwCq3/A==");
        yield return new(new TestStruct("123"), "I8cfdMmmdnMmTnLWCNfPGw==");
        yield return new(new TestFixedStruct(TimeSpan.FromSeconds(1)), "1Id5x7sfWT0zt6oIGiwBLw==");
        yield return new(TestEnum.One, "BsLOoYZ51kOZeDdI+jZ73Q==");
    }
}
