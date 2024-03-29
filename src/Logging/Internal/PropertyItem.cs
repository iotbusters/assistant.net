﻿using System.IO;

namespace Assistant.Net.Internal;

internal sealed class PropertyItem : Item
{
    private readonly string name;
    private readonly IItem value;

    public PropertyItem(string name, IItem value)
    {
        this.name = name;
        this.value = value;
    }

    public override void WriteTo(TextWriter writer, int indent, bool tryJoin)
    {
        if (value == Item.Nothing)
            return;

        base.WriteTo(writer, indent, tryJoin);
        writer.Write(name);
        writer.Write(":");
        value.WriteTo(writer, indent + 1, tryJoin: false);
    }
}
