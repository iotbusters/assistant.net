using Assistant.Net.Abstractions;
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
    public HashSet<string> ExcludedNamespaces { get; } = new()
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
    public HashSet<Type> ExcludedTypes { get; } = new();

    /// <summary>
    ///     Types to be included only.
    /// </summary>
    public HashSet<Type> IncludedTypes { get; } = new();
}
