using Assistant.Net.Options;
using System;
using System.Reflection;

namespace Assistant.Net;

/// <summary>
///     Type encoder configuration extensions.
/// </summary>
public static class TypeEncoderOptionsExtensions
{
    /// <summary>
    ///     Excludes all types from <paramref name="namespace"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="namespace">Type namespace to ignore.</param>
    public static TypeEncoderOptions Exclude(this TypeEncoderOptions options, string @namespace)
    {
        options.ExcludedNamespaces.Add(@namespace);
        return options;
    }

    /// <summary>
    ///     Excludes <typeparamref name="T"/> type.
    /// </summary>
    public static TypeEncoderOptions Exclude<T>(this TypeEncoderOptions options) => options
        .Exclude(typeof(T));

    /// <summary>
    ///     Excludes <paramref name="types"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="types">Types to ignore.</param>
    public static TypeEncoderOptions Exclude(this TypeEncoderOptions options, params Type[] types)
    {
        foreach (var type in types)
            options.ExcludedTypes.Add(type);
        return options;
    }

    /// <summary>
    ///     Excludes all types from <paramref name="assemblies"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="assemblies">Assembly types to ignore.</param>
    public static TypeEncoderOptions Exclude(this TypeEncoderOptions options, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
            options.Exclude(assembly.GetType());
        return options;
    }

    /// <summary>
    ///     Includes any type except <c>Excluded*</c> configurations.
    /// </summary>
    public static TypeEncoderOptions IncludeAnyType(this TypeEncoderOptions options)
    {
        options.IncludedTypes.Clear();
        return options;
    }

    /// <summary>
    ///     Includes <typeparamref name="T"/> type.
    /// </summary>
    public static TypeEncoderOptions Include<T>(this TypeEncoderOptions options) => options
        .Include(typeof(T));

    /// <summary>
    ///     Includes <paramref name="types"/>.
    /// </summary>
    public static TypeEncoderOptions Include(this TypeEncoderOptions options, params Type[] types)
    {
        foreach (var type in types)
            options.IncludedTypes.Add(type);
        return options;
    }

    /// <summary>
    ///     Includes all types from <paramref name="assemblies"/>.
    /// </summary>
    public static TypeEncoderOptions Include(this TypeEncoderOptions options, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
            options.Include(assembly.GetTypes());
        return options;
    }

    /// <summary>
    ///     Removes <paramref name="namespace"/> from excludes.
    /// </summary>
    public static TypeEncoderOptions Ensure(this TypeEncoderOptions options, string @namespace)
    {
        options.ExcludedNamespaces.Remove(@namespace);
        return options;
    }

    /// <summary>
    ///     Removes <typeparamref name="T"/> from excludes.
    /// </summary>
    public static TypeEncoderOptions Ensure<T>(this TypeEncoderOptions options) => options
        .Exclude(typeof(T));

    /// <summary>
    ///     Removes <paramref name="types"/> from excludes.
    /// </summary>
    public static TypeEncoderOptions Ensure(this TypeEncoderOptions options, params Type[] types)
    {
        foreach (var type in types)
            options.ExcludedTypes.Remove(type);
        return options;
    }

    /// <summary>
    ///     Removes <paramref name="assembly"/> from excludes.
    /// </summary>
    public static TypeEncoderOptions Ensure(this TypeEncoderOptions options, Assembly assembly) => options
        .Ensure(assembly.GetTypes());
}
