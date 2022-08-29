using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Reflection.BindingFlags;

namespace Assistant.Net.Logging.Internal;

internal abstract class Item : IItem
{
    public virtual void WriteTo(TextWriter writer, int indent, bool tryJoin)
    {
        if (tryJoin)
            writer.Write(" ");
        else
        {
            writer.WriteLine();
            writer.Write(Padding(indent));
        }
    }

    public static IItem Create(object? value)
    {
        if (value == null)
            return NullItem.Instance;

        if (value is IComparable or IFormattable)
            return new ValueItem(value);

        if (value is IEnumerable<byte> b)
        {
            var base64String = Convert.ToBase64String(b.ToArray());
            return new ValueItem(base64String);
        }

        if (value is IEnumerable items)
            return new ArrayItem(items.Cast<object>().Select(Create));

        var type = value.GetType();
        var hasToStringMethod = type.GetMethod(nameof(ToString), Type.EmptyTypes)?.DeclaringType == type;
        if (hasToStringMethod)
            return new ValueItem(value);

        var properties = new List<PropertyItem> { new("Type", new ValueItem(type.FullName!)) };
        foreach (var property in type.GetProperties(Instance | Public | GetProperty))
        {
            var propertyValue = property.GetMethod!.Invoke(value, Array.Empty<object?>());
            if (propertyValue != null)
                properties.Add(new(property.Name, new ValueItem(propertyValue)));
        }

        return new ObjectItem(properties.ToArray());
    }

    public static IItem Create(IEnumerable<PropertyItem>? properties)
    {
        if (properties == null)
            return NullItem.Instance;

        var array = properties.ToArray();
        if (!array.Any())
            return NullItem.Instance;

        return new ObjectItem(array);
    }

    public static IItem Create(IEnumerable<IItem>? items)
    {
        if (items == null)
            return NullItem.Instance;

        var array = items.ToArray();
        if (!array.Any())
            return NullItem.Instance;

        return new ArrayItem(array);
    }

    protected static string Padding(int indent) => new(' ', indent * 2);
}
