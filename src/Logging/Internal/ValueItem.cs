using System;
using System.Globalization;
using System.IO;

namespace Assistant.Net.Logging.Internal;

internal sealed class ValueItem : IItem
{
    private readonly object value;

    public ValueItem(object? value) =>
        this.value = value ?? throw new ArgumentNullException(nameof(value));

    public void WriteTo(TextWriter writer, int indent, bool _)
    {
        var valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
        writer.Write(" ");
        writer.Write(valueString);
    }
}
