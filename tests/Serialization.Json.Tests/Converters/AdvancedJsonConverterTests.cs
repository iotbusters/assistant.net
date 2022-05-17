using Assistant.Net.Serialization.Converters;
using Assistant.Net.Serialization.Json.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Assistant.Net.Serialization.Json.Tests.Converters;

public class AdvancedJsonConverterTests
{
    private static IServiceProvider Provider { get; } = new ServiceCollection()
        .AddTypeEncoder()
        .AddTransient(typeof(AdvancedJsonConverter<>), typeof(AdvancedJsonConverter<>))
        .BuildServiceProvider();

    [TestCase(typeof(ServiceDescriptor), TestName = "CanConvert_true(some.microsoft.type)")]
    [TestCase(typeof(TestClass), TestName = "CanConvert_true(some.custom.type)")]
    public void CanConvert_true_otherType(Type otherType)
    {
        ((JsonConverter?) ActivatorUtilities.CreateInstance(
                Provider,
                typeof(AdvancedJsonConverter<>).MakeGenericType(otherType)))
            ?.CanConvert(otherType).Should().BeTrue();
    }

    [TestCaseSource(nameof(SupportedObjects))]
    public async Task DeserializeObject(object value)
    {
        var converterType = typeof(AdvancedJsonConverter<>).MakeGenericType(value.GetType());
        var converter = (JsonConverter) ActivatorUtilities.CreateInstance(Provider, converterType);
        var options = new JsonSerializerOptions
        {
            Converters = {converter, new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false)}
        };

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
            {TestName = "DeserializeObject(Initialize+Ctor)"};

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
            {TestName = "DeserializeObject(Initialize+Ctor+Nullable)"};

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
            {TestName = "DeserializeObject(Initialize+Ctor+NullableArrays)"};

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
            {TestName = "DeserializeObject(Initialize+Setters)"};

        yield return new TestCaseData(
                new TestObjectWithPropertyInitialization
                {
                    StringArray = new[] {"test"}.AsEnumerable(),
                    ObjectArray = new[] {new TestClass(DateTime.Now)}
                })
            {TestName = "DeserializeObject(Initialize+Setters+Nullable)"};

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
            {TestName = "DeserializeObject(Initialize+Setters+NullableArrays)"};

        yield return new TestCaseData(
                new TestObjectWithMixedInitialization(TestEnum.A)
                {
                    String = "test"
                })
            {TestName = "DeserializeObject(Initialize)"};

        yield return new TestCaseData(
                new TestObjectWithMixedInitialization(null)
                {
                    String = "test"
                })
            {TestName = "DeserializeObject(Initialize+DefaultValue)"};

        yield return new TestCaseData(
                new TestObjectWithTypeCastingInitialization(TestEnum.A, 11, 12, new[] {"test"}))
            {TestName = "DeserializeObject(Initialize+TypeCasting)"};

        yield return new TestCaseData(
                new TestObjectWithTypeCastingInitialization(TestEnum.A, null, 12, new[] {"test"}))
            {TestName = "DeserializeObject(Initialize+TypeCasting+DefaultValue)"};

        yield return new TestCaseData(
                TestObjectWithPrivateCtor.New(TestEnum.A, "A", 12, 12f, 12m, DateTime.UtcNow))
            {TestName = "DeserializeObject(Initialize+PrivateCtor)"};
    }
}
