using System.IO;

namespace Assistant.Net.Logging.Internal;

internal sealed class PropertyItem : Item
{
    private readonly string name;
    private readonly IItem value;

    public PropertyItem(string name, IItem value)
    {
        this.name = name;
        this.value = value;
    }

    public PropertyItem(string name, object? value) : this(name, Create(value)) { }

    public override void WriteTo(TextWriter writer, int indent, bool tryJoin)
    {
        if (value == NullItem.Instance)
            return;

        base.WriteTo(writer, indent, tryJoin);
        writer.Write(name);
        writer.Write(":");
        value.WriteTo(writer, indent + 1, tryJoin: false);
    }
}
