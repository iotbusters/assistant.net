﻿using Assistant.Net.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Assistant.Net.Options;

/// <summary>
///     <see cref="ITypeEncoder"/> configuration.
/// </summary>
public class TypeEncoderOptions
{
    /// <summary>
    ///     Type namespaces to be ignored.
    /// </summary>
    public List<string> ExcludedNamespaces = new()
    {
        "Assistant.Net.Diagnostics",
        "Assistant.Net.Interceptors",
        "Assistant.Net.Options",
        "Microsoft",
        "System.ComponentModel",
        "System.Configuration",
        "System.Data",
        "System.Diagnostics",
        "System.Reflection",
        "System.Runtime",
        "System.Threading",
        "System.Xml"
    };

    /// <summary>
    ///     Types to be ignored.
    /// </summary>
    public List<Type> ExcludedTypes = new();

    /// <summary>
    ///     Assemblies which types to be ignored.
    /// </summary>
    public List<Assembly> ExcludedAssemblies = new();

    /// <summary>
    ///     Types to be included only.
    /// </summary>
    public List<Type> IncludedTypes = new();

    /// <summary>
    ///     Assemblies which types to be included only.
    /// </summary>
    public List<Assembly> IncludedAssemblies = new();
}
