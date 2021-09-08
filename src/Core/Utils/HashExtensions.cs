using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace Assistant.Net.Utils
{
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
        public static string GetSha1<T>(this T value)
        {
            if (typeof(T).IsValueType)
                return GetStructureSha1(value!);
            return GetClassSha1(value!);
        }

        /// <summary>
        ///     Generates <see cref="SHA1"/> hash code from any reference type.
        /// </summary>
        private static string GetClassSha1(this object value) => SerializeClass(value).GetSha1();

        /// <summary>
        ///     Generates <see cref="SHA1"/> hash code from <typeparamref name="T"/> value type.
        /// </summary>
        private static string GetStructureSha1<T>(this T value) => SerializeStructure(value).GetSha1();

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

        private static byte[] SerializeClass(this object value) =>
            JsonSerializer.SerializeToUtf8Bytes(value, value.GetType());
    }
}