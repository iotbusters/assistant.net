using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Assistant.Net.Utils;

/// <summary>
///     Hash code generating extensions.
/// </summary>
public static class HashExtensions
{
    /// <summary>
    ///     Generates <see cref="SHA1"/> hash code from <paramref name="stream"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetSha1(this Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        stream.Position = 0;
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(stream);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    ///     Generates <see cref="SHA1"/> hash code from <paramref name="bytes"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetSha1(this byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));

        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    ///     Generates <see cref="SHA1"/> hash code from <paramref name="text"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetSha1(this string text) =>
        text != null
            ? Encoding.UTF8.GetBytes(text).GetSha1()
            : throw new ArgumentNullException(nameof(text));

    /// <summary>
    ///     Generates <see cref="SHA1"/> hash code from <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetSha1<T>(this T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        using Stream stream = new MemoryStream();
        stream.Write(value);
        return stream.GetSha1();
    }

    /// <summary>
    ///     Generates <see cref="SHA256"/> hash code from <paramref name="stream"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetSha256(this Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        stream.Position = 0;
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(stream);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    ///     Generates <see cref="SHA256"/> hash code from <paramref name="bytes"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetSha256(this byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    ///     Generates <see cref="SHA256"/> hash code from <paramref name="text"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetSha256(this string text) =>
        text != null
            ? Encoding.UTF8.GetBytes(text).GetSha256()
            : throw new ArgumentNullException(nameof(text));

    /// <summary>
    ///     Generates <see cref="SHA256"/> hash code from <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetSha256<T>(this T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        using Stream stream = new MemoryStream();
        stream.Write(value);
        return stream.GetSha256();
    }

    /// <summary>
    ///     Generates <see cref="MD5"/> hash code from <paramref name="stream"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetMd5(this Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        stream.Position = 0;
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(stream);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    ///     Generates <see cref="MD5"/> hash code from <paramref name="bytes"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetMd5(this byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes));

        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    ///     Generates <see cref="MD5"/> hash code from <paramref name="text"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetMd5(this string text) =>
        text != null
            ? Encoding.UTF8.GetBytes(text).GetMd5()
            : throw new ArgumentNullException(nameof(text));

    /// <summary>
    ///     Generates <see cref="MD5"/> hash code from <paramref name="value"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public static string GetMd5<T>(this T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        using Stream stream = new MemoryStream();
        stream.Write(value);
        return stream.GetMd5();
    }

    private static void Write<T>(this Stream stream, T value)
    {
        switch (value)
        {
            case null:
                return;
            case ValueType x when x.GetType().IsPrimitive:
                stream.Write(SerializePrimitive(value));
                return;
            case Guid x:
                stream.Write(x.ToByteArray());
                return;
            case TimeSpan x:
                stream.Write(BitConverter.GetBytes(x.Ticks));
                return;
            case DateTime x:
                stream.Write(BitConverter.GetBytes(x.ToUniversalTime().Ticks));
                return;
            case DateTimeOffset x:
                stream.Write(BitConverter.GetBytes(x.ToUniversalTime().Ticks));
                return;
            case string x:
                stream.Write(Encoding.UTF8.GetBytes(x));
                return;
            case Enum x:
                stream.Write(Encoding.UTF8.GetBytes(x.ToString()));
                return;
            case Stream x:
                x.Position = 0;
                x.CopyToAsync(stream);
                return;
            case ISerializable x:
                var info = new SerializationInfo(x.GetType(), new FormatterConverter());
                x.GetObjectData(info, new(StreamingContextStates.Clone));
                foreach (var entry in info) stream.Write(entry.Value);
                return;
            case IEnumerable<byte> x:
                stream.Write(x.ToArray());
                return;
            case IEnumerable x:
                foreach (var element in x) stream.Write(element);
                return;
            default:
                var type = value.GetType();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (!fields.Any())
                {
                    stream.Write(JsonSerializer.SerializeToUtf8Bytes(value));
                    return;
                }

                stream.Write(Encoding.UTF8.GetBytes(type.FullName!));
                foreach (var field in fields)
                    stream.Write(field.GetValue(value));

                return;
        }
    }

    private static byte[] SerializePrimitive<T>(this T value)
    {
        var size = Marshal.SizeOf(value!.GetType());
        var bytes = new byte[size];
        var pointer = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(value, pointer, true);
        Marshal.Copy(pointer, bytes, 0, size);
        Marshal.FreeHGlobal(pointer);

        return bytes;
    }
}
