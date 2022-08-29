using System.IO;

namespace Assistant.Net.Logging.Internal;

internal sealed class NullItem : IItem
{
    private NullItem() { }

    public static NullItem Instance { get; } = new();

    public void WriteTo(TextWriter writer, int indent, bool tryJoin) { }
}
