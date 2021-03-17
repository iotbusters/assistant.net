using System;
using Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Assistant.Net.Messaging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRequestClient(this IServiceCollection services, Action<RequestConfigurationBuilder> configure = null!)
        {
            var builder = new RequestConfigurationBuilder(services);
            configure?.Invoke(builder);
            return services
                .AddScoped<IRequestClient, RequestClient>(p => p);
        }
    }
}