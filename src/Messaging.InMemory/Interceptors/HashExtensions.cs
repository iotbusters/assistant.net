using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Assistant.Net.Messaging.Interceptors
{
    /// <summary>
    ///     Hash code generating extensions.
    /// </summary>
    internal static class HashExtensions
    {
        public static string GetSha1(this object target)
        {
            var value = JsonSerializer.Serialize(target);
            var bytes = Encoding.UTF8.GetBytes(value);

            using var sha = SHA1.Create();

            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}