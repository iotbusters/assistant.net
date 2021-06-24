using System.Runtime.InteropServices;
using System.Text.Json;

namespace Assistant.Net.Utils
{
    /// <summary>
    ///     Object serialization extensions.
    /// </summary>
    internal static class SerializationExtensions
    {
        /// <summary>
        ///     Serializes <param name="value" /> to byte array by internal strategy.
        /// </summary>
        public static byte[] Serialize<T>(this T value)
        {
            if (typeof(T).IsValueType)
                return SerializeStructure(value);
            return SerializeClass(value);
        }

        private static byte[] SerializeStructure<T>(this T value)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] bytes = new byte[size];
            var pointer = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(value!, pointer, true);
            Marshal.Copy(pointer, bytes, 0, size);
            Marshal.FreeHGlobal(pointer);

            return bytes;
        }

        private static byte[] SerializeClass<T>(this T value) =>
            JsonSerializer.SerializeToUtf8Bytes(value);
    }
}