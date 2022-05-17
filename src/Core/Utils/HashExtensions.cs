using System;
using System.Linq;
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

        // todo: replace JsonSerializer with custom binary serialization.
        var typePrefix = Encoding.UTF8.GetBytes(value.GetType().Name);
        var bodySuffix = JsonSerializer.SerializeToUtf8Bytes(value, value.GetType());
        return typePrefix.Concat(bodySuffix).ToArray().GetSha1();
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