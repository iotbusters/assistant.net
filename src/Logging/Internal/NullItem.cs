using System.IO;

namespace Assistant.Net.Internal;

internal sealed class NullItem : IItem
{
    public void WriteTo(TextWriter writer, int indent, bool tryJoin) { }
}
