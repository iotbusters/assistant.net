using Assistant.Net.Messaging.Serialization;
using Assistant.Net.Serialization;
using Assistant.Net.Serialization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging
{
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