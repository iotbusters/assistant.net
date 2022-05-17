using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Assistant.Net.Serialization.Json.Tests.Mocks;

public class TestObjectWithTypeCastingInitialization
{
    public TestObjectWithTypeCastingInitialization(
        TestEnum @enum,
        int? number1,
        int number2,
        IEnumerable<string?> stringArray)
    {
        Enum = @enum;
        Number1 = number1 ?? default;
        Number2 = number2;
        StringArray = stringArray.Select(x => x ?? string.Empty).ToImmutableList();
    }

    public TestEnum? Enum { get; set; } = null;
    public int Number1 { get; set; } = default;
    public int? Number2 { get; set; } = default;
    public IReadOnlyCollection<string>? StringArray { get; set; } = null!;
}
