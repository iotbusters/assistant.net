using System.IO;

namespace Assistant.Net.Logging.Internal;

internal interface IItem
{
    public void WriteTo(TextWriter writer, int indent, bool tryJoin);
}
