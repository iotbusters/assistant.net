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
        public static string GetSha1<T>(this T value)
        {
            if (typeof(T).IsValueType)
                return GetStructureSha1((dynamic)value!);
            return GetClassSha1((dynamic)value!);
        }

        public static string GetClassSha1<T>(this T value) where T : class
        {
            using var sha = SHA1.Create();

            var payload = JsonSerializer.Serialize(value);
            var bytes = Encoding.UTF8.GetBytes(payload);
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        public static string GetStructureSha1<T>(this T value) where T : struct
        {
            using var sha = SHA1.Create();

            var bytes = value.Serialize();
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}