using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Assistant.Net.Serialization.Converters;

internal class TypeMetadata
{
    public TypeMetadata(
        ConstructorInfo ctor,
        IImmutableList<string> ctorArguments,
        IImmutableDictionary<string, PropertyInfo> getters,
        IImmutableDictionary<string, PropertyInfo> setters)
    {
        Ctor = ctor;
        CtorArguments = ctorArguments;
        Getters = getters;
        Setters = setters;
    }

    public ConstructorInfo Ctor { get; }

    public IImmutableList<string> CtorArguments { get; }

    public IImmutableDictionary<string, PropertyInfo> Getters { get; }

    public IImmutableDictionary<string, PropertyInfo> Setters { get; }
}