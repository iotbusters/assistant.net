using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Serialization;
using Assistant.Net.Serialization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging
{
    /// <summary>
    ///     Service collection extensions for json serialization.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers default <see cref="ISerializer{TValue}" /> configuration.
        /// </summary>
        public static IServiceCollection AddJsonSerialization(this IServiceCollection services) => services
            .AddSerializer(b => b
                .AddJsonConverter<CommandExceptionJsonConverter>()
                .AddJsonTypeAny());
    }
}