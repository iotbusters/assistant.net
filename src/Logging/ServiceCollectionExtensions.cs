using Assistant.Net.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Assistant.Net;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds logging services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    public static IServiceCollection AddLogging(this IServiceCollection services) =>
        AddLogging(services, null!);

    /// <summary>
    ///     Adds logging services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configure">The <see cref="ILoggingBuilder"/> configuration delegate.</param>
    public static IServiceCollection AddLogging(this IServiceCollection services, Action<ILoggingBuilder>? configure)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services
            .Configure<LoggerFilterOptions>(o => o.MinLevel = LogLevel.Information)
            .TryAddSingleton<ILoggerFactory, DefaultLoggerFactory>()
            .TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));

        configure?.Invoke(new LoggingBuilder(services));
        return services;
    }
}
