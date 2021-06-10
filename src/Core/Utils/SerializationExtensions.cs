using System.Runtime.InteropServices;
using System.Text.Json;

namespace Assistant.Net.Utils
{
    /// <summary>
    ///     Object serialization extensions.
    /// </summary>
    public static class SerializationExtensions
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

        /// <summary>
        ///     Deserializes <param name="bytes" /> to <typeparamref name="T"/> by internal strategy.
        /// </summary>
        public static T Deserialize<T>(this byte[] bytes)
        {
            if (typeof(T).IsValueType)
                return DeserializeStructure<T>(bytes);
            return DeserializeClass<T>(bytes);
        }

        internal static byte[] SerializeStructure<T>(this T value)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] bytes = new byte[size];
            var pointer = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(value!, pointer, true);
            Marshal.Copy(pointer, bytes, 0, size);
            Marshal.FreeHGlobal(pointer);

            return bytes;
        }

        internal static T DeserializeStructure<T>(this byte[] bytes)
        {
            var size = Marshal.SizeOf(typeof(T));
            var pointer = Marshal.AllocHGlobal(size);

            Marshal.Copy(bytes, 0, pointer, size);
            var value = (T)Marshal.PtrToStructure(pointer,  typeof(T))!;
            Marshal.FreeHGlobal(pointer);

            return value;
        }

        internal static byte[] SerializeClass<T>(this T value) =>
            JsonSerializer.SerializeToUtf8Bytes(value);

        internal static T DeserializeClass<T>(this byte[] value) =>
            JsonSerializer.Deserialize<T>(value)!;
    }
}