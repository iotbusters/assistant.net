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
    public List<string> ExcludedNamespaces = new()
    {
        "Assistant.Net.Diagnostics",
        "Assistant.Net.Interceptors",
        "Assistant.Net.Options",
        "Assistant.Net.Unions",
        "Microsoft",
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
    public List<Assembly> ExcludedAssembly = new();
}
