using System.Collections.Generic;
using System.IO;

namespace Assistant.Net.Internal;

internal sealed class ArrayItem : Item
{
    private readonly IEnumerable<IItem> items;

    public ArrayItem(IEnumerable<IItem> items) =>
        this.items = items;

    public override void WriteTo(TextWriter writer, int indent, bool tryJoin)
    {
        var first = tryJoin;
        foreach (var item in items)
        {
            base.WriteTo(writer, indent, first);
            writer.Write("-");
            item.WriteTo(writer, indent + 1, tryJoin: true);
            first = false;
        }
    }
}
