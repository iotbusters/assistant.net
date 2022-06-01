using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Json.Tests.Mocks;

namespace Assistant.Net.Serialization.Json.Tests.Converters;

public class AdvancedJsonConverterFactoryTests
{
    private readonly JsonSerializerOptions options = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false),
            Provider.GetRequiredService<AdvancedJsonConverterFactory>()
        }
    };

    private static IServiceProvider Provider { get; } = new ServiceCollection()
        .AddTypeEncoder()
        .AddSingleton<AdvancedJsonConverterFactory>()
        .TryAddSingleton(typeof(EnumerableJsonConverter<>), typeof(EnumerableJsonConverter<>))
        .TryAddSingleton(typeof(AdvancedJsonConverter<>), typeof(AdvancedJsonConverter<>))
        .BuildServiceProvider();

    [TestCase(typeof(ServiceDescriptor), TestName = "CanConvert_true(some.microsoft.type)")]
    [TestCase(typeof(TestClass), TestName = "CanConvert_true(some.custom.type)")]
    [TestCase(typeof(Exception), TestName = "CanConvert_true(Exception)")]
    public void CanConvert_true_otherType(Type otherType) =>
        new AdvancedJsonConverterFactory(null!).CanConvert(otherType).Should().BeTrue();

    [TestCase(typeof(string), TestName = "CanConvert_false(String)")]
    [TestCase(typeof(int), TestName = "CanConvert_false(Int32)")]
    [TestCase(typeof(byte), TestName = "CanConvert_false(Byte)")]
    [TestCase(typeof(DateTime), TestName = "CanConvert_false(DateTime)")]
    [TestCase(typeof(object), TestName = "CanConvert_false(Object)")]
    [TestCase(typeof(List<string>), TestName = "CanConvert_false(List)")]
    [TestCase(typeof(Type), TestName = "CanConvert_false(Type)")]
    [TestCase(typeof(IEnumerable<string>), TestName = "CanConvert_false(Enumerable)")]
    public void CanConvert_false_systemType(Type systemType) =>
        new AdvancedJsonConverterFactory(null!).CanConvert(systemType).Should().BeFalse();

    [TestCaseSource(nameof(SupportedObjects))]
    public async Task DeserializeObject(object value)
    {
        await using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value, options);
        stream.Position = 0;

        var deserialized = await JsonSerializer.DeserializeAsync(stream, value.GetType(), options);

        deserialized.Should().BeOfType(value.GetType())
            .And.BeEquivalentTo(value);
    }

    private static IEnumerable<TestCaseData> SupportedObjects()
    {
        yield return new TestCaseData(
                new TestObjectWithCtorInitialization(
                    TestEnum.A,
                    "test",
                    11,
                    11.1f,
                    11.11m,
                    DateTime.Now,
                    new int?[] {1},
                    new[] {"test"}.AsEnumerable(),
                    new[] {new TestClass(DateTime.Now)}))
            {TestName = "DeserializeObject(Initialize+Ctor)" };

        yield return new TestCaseData(
                new TestObjectWithCtorInitialization(
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    new int?[] {1},
                    new[] {"test"}.AsEnumerable(),
                    new[] {new TestClass(DateTime.Now)}))
            {TestName = "DeserializeObject(Initialize+Ctor+Nullable)" };

        yield return new TestCaseData(
                new TestObjectWithCtorInitialization(
                    TestEnum.A,
                    "test",
                    11,
                    11.1f,
                    11.11m,
                    DateTime.Now,
                    new int?[] {null, 11},
                    new[] {null, "test"}.AsEnumerable(),
                    new[] {null, new TestClass(DateTime.Now)}))
            {TestName = "DeserializeObject(Initialize+Ctor+NullableArrays)" };

        yield return new TestCaseData(
                new TestObjectWithPropertyInitialization
                {
                    Enum = TestEnum.A,
                    String = "test",
                    IntegerNumber = 11,
                    FloatNumber = 11.1f,
                    DecimalNumber = 11.11m,
                    DateTime = DateTime.Now,
                    StringArray = new[] {"test"}.AsEnumerable(),
                    ObjectArray = new[] {new TestClass(DateTime.Now)}
                })
            {TestName = "DeserializeObject(Initialize+Setters)" };

        yield return new TestCaseData(
                new TestObjectWithPropertyInitialization
                {
                    StringArray = new[] {"test"}.AsEnumerable(),
                    ObjectArray = new[] {new TestClass(DateTime.Now)}
                })
            {TestName = "DeserializeObject(Initialize+Setters+Nullable)" };

        yield return new TestCaseData(
                new TestObjectWithPropertyInitialization
                {
                    Enum = TestEnum.A,
                    String = "test",
                    IntegerNumber = 11,
                    FloatNumber = 11.1f,
                    DecimalNumber = 11.11m,
                    DateTime = DateTime.Now,
                    IntegerArray = new int?[] {null, 11},
                    StringArray = new[] {null, "test"}.AsEnumerable(),
                    ObjectArray = new[] {null, new TestClass(DateTime.Now)}
                })
            {TestName = "DeserializeObject(Initialize+Setters+NullableArrays)" };

        yield return new TestCaseData(
                new TestObjectWithMixedInitialization(TestEnum.A)
                {
                    String = "test"
                })
            {TestName = "DeserializeObject(Initialize)" };

        yield return new TestCaseData(
                new TestObjectWithMixedInitialization(null)
                {
                    String = "test"
                })
            {TestName = "DeserializeObject(Initialize+DefaultValue)" };

        yield return new TestCaseData(
                new TestObjectWithTypeCastingInitialization(TestEnum.A, 11, 12, new[] {"test"}))
            {TestName = "DeserializeObject(Initialize+TypeCasting)" };

        yield return new TestCaseData(
                new TestObjectWithTypeCastingInitialization(TestEnum.A, null, 12, new[] {"test"}))
            {TestName = "DeserializeObject(Initialize+TypeCasting+DefaultValue)" };
    }
}
