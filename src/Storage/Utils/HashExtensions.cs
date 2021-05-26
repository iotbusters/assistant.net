using System;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace Assistant.Net.Storage.Utils
{
    /// <summary>
    ///     Hash code generating extensions.
    /// </summary>
    public static class HashExtensions
    {
        /// <summary>
        ///     Generates <see cref="SHA1"> hash code from <paramref name="value"/>.
        /// </summary>
        public static string GetSha1<T>(this T value)
        {
            if (typeof(T).IsValueType)
                return GetStructureSha1((dynamic)value!);
            return GetClassSha1((dynamic)value!);
        }

        /// <summary>
        ///     Generates <see cref="SHA1"> hash code from <typeparamref name="T"/> reference type.
        /// </summary>
        public static string GetClassSha1<T>(this T value) where T : class
        {
            using var sha = SHA1.Create();

            var payload = JsonSerializer.Serialize(value);
            var bytes = Encoding.UTF8.GetBytes(payload);
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        ///     Generates <see cref="SHA1"> hash code from <typeparamref name="T"/> value type.
        /// </summary>
        public static string GetStructureSha1<T>(this T value) where T : struct
        {
            using var sha = SHA1.Create();

            var bytes = value.Serialize();
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}