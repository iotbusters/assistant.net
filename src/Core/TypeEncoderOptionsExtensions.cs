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
        options.ExcludedAssembly.Add(assembly);
        return options;
    }
}