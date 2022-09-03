using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Assistant.Net.Internal;

internal sealed class ValueItem : Item
{
    private readonly object value;

    public ValueItem(object? value) =>
        this.value = value ?? throw new ArgumentNullException(nameof(value));

    public override void WriteTo(TextWriter writer, int indent, bool _)
    {
        var valueString = Convert.ToString(value, CultureInfo.InvariantCulture)!;

        writer.Write(" ");

        if (!valueString.Contains(writer.NewLine))
        {
            writer.Write(valueString.TrimStart());
            return;
        }

        writer.Write("|");

        var array = valueString.Trim(writer.NewLine.ToCharArray()).Split(writer.NewLine);
        var minIndent = array.Select(x => x.TakeWhile(c => c == ' ').Count()).Min();

        foreach (var line in array)
        {
            writer.WriteLine();
            writer.Write(Padding(indent));
            writer.Write(line[minIndent..]);
        }
    }
}
