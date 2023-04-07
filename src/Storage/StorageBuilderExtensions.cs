using Assistant.Net.Options;
using Assistant.Net.Serialization;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Internal;
using Assistant.Net.Storage.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Assistant.Net.Storage;

/// <summary>
///     Storage builder extensions for configuring local storages.
/// </summary>
public static class StorageBuilderExtensions
{
    /// <summary>
    ///     Configures storages to use a local storage implementations.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered storage provider.
    /// </remarks>
    public static StorageBuilder UseLocal(this StorageBuilder builder)
    {
        builder.Services
            .AddLocalProvider()
            .ConfigureStorageOptions(builder.Name, o => o.UseLocal());
        return builder;
    }

    /// <summary>
    ///     Adds storages of <typeparamref name="TValue"/> by <typeparamref name="TKey"/> type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder Add<TKey, TValue>(this StorageBuilder builder) => builder
        .Add(typeof(TKey), typeof(TValue));

    /// <summary>
    ///     Adds storages of <paramref name="valueType"/> by <paramref name="keyType"/>.
    /// </summary>
    /// <remarks>
    ///     Pay attention, the method overrides already registered provider.
    /// </remarks>
    public static StorageBuilder Add(this StorageBuilder builder, Type keyType, Type valueType)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AddType(valueType))
            .ConfigureSerializer(builder.Name, b => b.AddType(keyType).AddType(valueType));
        return builder;
    }

    /// <summary>
    ///     Allows storages of any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, specific types configured by <see cref="Add"/> methods will be ignored.
    /// </remarks>
    public static StorageBuilder AllowAnyType(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.AllowAnyType())
            .ConfigureSerializer(builder.Name, b => b.AllowAnyType());
        return builder;
    }

    /// <summary>
    ///     Disallows storages of any unregistered type.
    /// </summary>
    /// <remarks>
    ///     Pay attention, specific types should be configured by <see cref="Add"/> methods.
    /// </remarks>
    public static StorageBuilder DisallowAnyType(this StorageBuilder builder)
    {
        builder.Services
            .ConfigureStorageOptions(builder.Name, o => o.DisallowAnyType())
            .ConfigureSerializer(builder.Name, b => b.DisallowAnyType());
        return builder;
    }

    /// <summary>
    ///     Apply a configuration type <typeparamref name="TConfiguration"/>.
    /// </summary>
    public static StorageBuilder AddConfiguration<TConfiguration>(this StorageBuilder builder)
        where TConfiguration : IStorageConfiguration, new() => builder.AddConfiguration(new TConfiguration());

    /// <summary>
    ///     Apply a list of configuration instances <paramref name="storageConfigurations"/>.
    /// </summary>
    public static StorageBuilder AddConfiguration(this StorageBuilder builder, params IStorageConfiguration[] storageConfigurations)
    {
        foreach (var config in storageConfigurations)
            config.Configure(builder);
        return builder;
    }

    /// <summary>
    ///     Adds custom value <typeparamref name="TConverter"/> type implementing <see cref="IValueConverter{TValue}"/>.
    /// </summary>
    public static StorageBuilder AddConverter<TConverter>(this StorageBuilder builder) where TConverter : class => builder
        .AddConverter(typeof(TConverter));

    /// <summary>
    ///     Adds custom value <paramref name="converterType"/> implementing <see cref="IValueConverter{TValue}"/>.
    /// </summary>
    public static StorageBuilder AddConverter(this StorageBuilder builder, Type converterType)
    {
        var convertingTypes = converterType.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IValueConverter<>))
            .Select(x => x.GetGenericArguments().Single())
            .ToHashSet();
        if (!convertingTypes.Any())
            throw new ArgumentException($"{converterType} doesn't implement IValueConverter<T>.", nameof(converterType));

        builder.Services.ConfigureStorageOptions(o =>
        {
            var factory = new InstanceCachingFactory<object>(p => p.GetService(converterType) ?? p.Create(converterType));
            foreach (var convertingType in convertingTypes)
                o.Converters[convertingType] = factory;
        });
        return builder;
    }

    /// <summary>
    ///     Adds custom value <paramref name="converter"/> implementing <see cref="IValueConverter{TValue}"/>.
    /// </summary>
    public static StorageBuilder AddConverter(this StorageBuilder builder, object converter)
    {
        var converterType = converter.GetType();
        var convertingTypes = converterType.GetInterfaces()
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IValueConverter<>))
            .Select(x => x.GetGenericArguments().Single())
            .ToHashSet();
        if (!convertingTypes.Any())
            throw new ArgumentException($"{converterType} doesn't implement IValueConverter<T>.", nameof(converterType));

        builder.Services.ConfigureStorageOptions(o =>
        {
            var factory = new InstanceCachingFactory<object>(_ => converter);
            foreach (var convertingType in convertingTypes)
                o.Converters[convertingType] = factory;
        });
        return builder;
    }

    private static IServiceCollection AddLocalProvider(this IServiceCollection services) => services
        .TryAddSingleton(typeof(LocalStorageProvider<>), typeof(LocalStorageProvider<>))
        .TryAddSingleton(typeof(LocalHistoricalStorageProvider<>), typeof(LocalHistoricalStorageProvider<>))
        .TryAddSingleton(typeof(LocalPartitionedStorageProvider<>), typeof(LocalPartitionedStorageProvider<>));
}
