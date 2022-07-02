using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace Assistant.Net.Utils;

/// <summary>
///     Hash code generating extensions.
/// </summary>
public static class HashExtensions
{
    /// <summary>
    ///     Generates <see cref="SHA1"/> hash code from <paramref name="bytes"/>.
    /// </summary>
    public static string GetSha1(this byte[] bytes)
    {
        using var sha = SHA1.Create();

        var hash = sha.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }

    /// <summary>
    ///     Generates <see cref="SHA1"/> hash code from <paramref name="text"/>.
    /// </summary>
    public static string GetSha1(this string text) => Encoding.UTF8.GetBytes(text).GetSha1();

    /// <summary>
    ///     Generates <see cref="SHA1"/> hash code from <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetSha1<T>(this T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (typeof(T).IsValueType)
            return SerializeStructure(value).GetSha1();

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(value);

        return stream.ToArray().GetSha1();
    }

    private static void Write<T>(this BinaryWriter writer, T? obj)
    {
        switch (obj)
        {
            case null:
                return;
            case bool x:
                writer.Write(x);
                return;
            case byte x:
                writer.Write(x);
                return;
            case short x:
                writer.Write(x);
                return;
            case int x:
                writer.Write(x);
                return;
            case long x:
                writer.Write(x);
                return;
            case float x:
                writer.Write(x);
                return;
            case double x:
                writer.Write(x);
                return;
            case decimal x:
                writer.Write(x);
                return;
            case byte[] x:
                writer.Write(x);
                return;
            case char[] x:
                writer.Write(x);
                return;
            case string x:
                writer.Write(x);
                return;
        }

        var type = obj.GetType();
        if (type.IsValueType)
        {
            writer.Write(SerializeStructure(obj));
            return;
        }

        if (obj is IEnumerable seq)
        {
            foreach (var element in seq)
                writer.Write(element);
            return;
        }

        writer.Write(Encoding.UTF8.GetBytes(type.FullName!));
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        if (properties.Any())
        {
            foreach (var property in properties)
                writer.Write(property.GetValue(obj));
            return;
        }

        writer.Write(JsonSerializer.SerializeToUtf8Bytes(obj));
    }

    private static byte[] SerializeStructure<T>(this T value)
    {
        var size = Marshal.SizeOf(typeof(T));
        var bytes = new byte[size];
        var pointer = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(value!, pointer, true);
        Marshal.Copy(pointer, bytes, 0, size);
        Marshal.FreeHGlobal(pointer);

        return bytes;
    }
}
