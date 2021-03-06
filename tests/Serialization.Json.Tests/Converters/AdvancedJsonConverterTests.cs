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

namespace Assistant.Net.Serialization.Json.Tests.Converters
{
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

        [TestCase(typeof(string), TestName = "NewConverterOfSystemTypeFailed(String)")]
        [TestCase(typeof(int), TestName = "NewConverterOfSystemTypeFailed(Int32)")]
        [TestCase(typeof(byte), TestName = "NewConverterOfSystemTypeFailed(Byte)")]
        [TestCase(typeof(DateTime), TestName = "NewConverterOfSystemTypeFailed(DateTime)")]
        [TestCase(typeof(object), TestName = "NewConverterOfSystemTypeFailed(Object)")]
        [TestCase(typeof(List<string>), TestName = "NewConverterOfSystemTypeFailed(List)")]
        [TestCase(typeof(IEnumerable<string>), TestName = "NewConverterOfSystemTypeFailed(Enumerable)")]
        [TestCase(typeof(Exception), TestName = "NewConverterOfSystemTypeFailed(Exception)")]
        [TestCase(typeof(Type), TestName = "NewConverterOfSystemTypeFailed(Type)")]
        public void NewConverterOfSystemType_failed(Type systemType)
        {
            this.Invoking(_ => (JsonConverter?) ActivatorUtilities.CreateInstance(
                    Provider,
                    typeof(AdvancedJsonConverter<>).MakeGenericType(systemType)))
                .Should().Throw<JsonException>()
                .WithMessage($"The type '{systemType}' isn't supported.");
        }

        [Test]
        public void NewConverterOfUnserializableType_failed()
        {
            this.Invoking(_ => (JsonConverter?) ActivatorUtilities.CreateInstance(
                    Provider,
                    typeof(AdvancedJsonConverter<TestObjectUnserializable>)))
                .Should().Throw<JsonException>()
                .WithMessage($"The type '{typeof(TestObjectUnserializable)}' cannot be serialized or deserialized.");
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
}