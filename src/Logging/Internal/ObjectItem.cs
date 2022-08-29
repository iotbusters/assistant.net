using System.IO;

namespace Assistant.Net.Logging.Internal;

internal sealed class ObjectItem : IItem
{
    private readonly PropertyItem[] properties;

    public ObjectItem(params PropertyItem[] properties) =>
        this.properties = properties;

    public void WriteTo(TextWriter writer, int indent, bool tryJoin)
    {
        var localTryJoin = tryJoin;
        foreach (var property in properties)
        {
            property.WriteTo(writer, indent, localTryJoin);
            localTryJoin = false;
        }
    }
}
