﻿using Assistant.Net.Options;
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
    ///     Excludes <paramref name="type"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="type">Type to ignore.</param>
    public static TypeEncoderOptions Exclude(this TypeEncoderOptions options, Type type)
    {
        options.ExcludedTypes.Add(type);
        return options;
    }

    /// <summary>
    ///     Excludes all types from <paramref name="assembly"/>.
    /// </summary>
    /// <param name="options"/>
    /// <param name="assembly">Type assembly to ignore.</param>
    public static TypeEncoderOptions Exclude(this TypeEncoderOptions options, Assembly assembly)
    {
        options.ExcludedAssemblies.Add(assembly);
        return options;
    }

    /// <summary>
    ///     Includes <typeparamref name="T"/> type.
    /// </summary>
    public static TypeEncoderOptions Include<T>(this TypeEncoderOptions options) => options
        .Include(typeof(T));

    /// <summary>
    ///     Includes <paramref name="type"/>.
    /// </summary>
    public static TypeEncoderOptions Include(this TypeEncoderOptions options, Type type)
    {
        options.IncludedTypes.Add(type);
        return options;
    }

    /// <summary>
    ///     Includes all types from <paramref name="assembly"/>.
    /// </summary>
    public static TypeEncoderOptions Include(this TypeEncoderOptions options, Assembly assembly)
    {
        options.IncludedAssemblies.Add(assembly);
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
    ///     Removes <paramref name="type"/> from excludes.
    /// </summary>
    public static TypeEncoderOptions Ensure(this TypeEncoderOptions options, Type type)
    {
        options.ExcludedTypes.Remove(type);
        return options;
    }

    /// <summary>
    ///     Removes <paramref name="assembly"/> from excludes.
    /// </summary>
    public static TypeEncoderOptions Ensure(this TypeEncoderOptions options, Assembly assembly)
    {
        options.ExcludedAssemblies.Remove(assembly);
        return options;
    }
}
