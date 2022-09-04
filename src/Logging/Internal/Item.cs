using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Reflection.BindingFlags;

namespace Assistant.Net.Internal;

internal abstract class Item : IItem
{
    public static IItem Nothing { get; } = new NullItem();

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

    public static IItem CreateValue(object? value)
    {
        if (value == null)
            return Item.Nothing;

        // array support
        if (value is IEnumerable enumerable && !IsValue(value))
            return CreateArray(enumerable);

        if (value is IEnumerable<byte> b)
            return new ValueItem(Convert.ToBase64String(b.ToArray()));

        return new ValueItem(value);
    }

    public static IItem CreateObject(object? value)
    {
        if (value == null)
            return Item.Nothing;

        if (IsValue(value))
            return CreateValue(value);

        // array support
        if (value is IEnumerable enumerable)
            return CreateArray(enumerable);

        try
        {
            var properties = new List<PropertyItem>();
            var type = value.GetType();
            foreach (var property in type.GetProperties(Instance | Public | GetProperty))
            {
                var propertyValue = property.GetMethod!.Invoke(value, Array.Empty<object?>());
                if (propertyValue != null)
                    properties.Add(new(property.Name, CreateObject(propertyValue)));
            }

            if (properties.Any())
                return CreateObject(properties);
        }
        catch
        {
            // suppress
        }

        // not an object
        return CreateValue(value);
    }

    public static IItem CreateObject(IEnumerable<PropertyItem>? properties)
    {
        if (properties == null)
            return Item.Nothing;

        var array = properties.ToArray();
        if (!array.Any())
            return Item.Nothing;

        return new ObjectItem(array);
    }

    public static IItem CreateArray(IEnumerable? enumerable)
    {
        if (enumerable == null)
            return Item.Nothing;

        var items = enumerable.Cast<object>().Select(CreateValue);
        return CreateArray(items);
    }

    public static IItem CreateArray(IEnumerable<IItem>? items)
    {
        if (items == null)
            return Item.Nothing;

        var array = items.Where(x => x != Item.Nothing).ToArray();
        if (!array.Any())
            return Item.Nothing;

        return new ArrayItem(array);
    }

    protected static string Padding(int indent) => new(' ', indent * 2);

    private static bool IsValue(object value) => value is IConvertible or IFormattable;
}
