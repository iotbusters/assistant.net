using System.IO;

namespace Assistant.Net.Internal;

internal interface IItem
{
    public void WriteTo(TextWriter writer, int indent, bool tryJoin);
}
