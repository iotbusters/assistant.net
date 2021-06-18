using Assistant.Net.Serialization.Abstractions;
using Assistant.Net.Serialization.Configuration;
using Assistant.Net.Serialization.Internal;

namespace Assistant.Net.Serialization
{
    public static class SerializerBuilderExtensions
    {
        public static SerializerBuilder AddJson<TValue>(this SerializerBuilder builder)
        {
            builder.Services.ReplaceScoped(typeof(ISerializer<TValue>), typeof(JsonSerializer<TValue>));
            return builder;
        }

        public static SerializerBuilder AddJsonAny(this SerializerBuilder builder)
        {
            builder.Services.ReplaceScoped(typeof(ISerializer<>), typeof(JsonSerializer<>));
            return builder;
        }
    }
}