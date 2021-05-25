using System.Runtime.InteropServices;
using System.Text.Json;

namespace Assistant.Net.Storage.Utils
{
    /// <summary>
    ///     Object serialization extensions.
    /// </summary>
    public static class SerializationExtensions
    {
        public static byte[] Serialize<T>(this T value)
        {
            if (typeof(T).IsValueType)
                return SerializeStructure((dynamic)value!);
            return SerializeClass((dynamic)value!);
        }

        public static T Deserialize<T>(this byte[] bytes)
        {
            if (typeof(T).IsValueType)
                return DeserializeStructure((dynamic)bytes!);
            return DeserializeClass((dynamic)bytes!);
        }

        public static byte[] SerializeStructure<T>(this T value) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] bytes = new byte[size];
            var pointer = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(value, pointer, true);
            Marshal.Copy(pointer, bytes, 0, size);
            Marshal.FreeHGlobal(pointer);

            return bytes;
        }

        public static T DeserializeStructure<T>(this byte[] bytes) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var pointer = Marshal.AllocHGlobal(size);

            Marshal.Copy(bytes, 0, pointer, size);
            var value = (T)Marshal.PtrToStructure(pointer,  typeof(T))!;
            Marshal.FreeHGlobal(pointer);

            return value;
        }

        public static byte[] SerializeClass<T>(this T value) where T : class =>
            JsonSerializer.SerializeToUtf8Bytes(value);

        public static T DeserializeClass<T>(this byte[] value) where T : class =>
            JsonSerializer.Deserialize<T>(value)!;
    }
}