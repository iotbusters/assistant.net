using Assistant.Net.Options;
using Assistant.Net.Serialization.Abstractions;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Serialization.Options;

/// <summary>
///     Serializer configuration.
/// </summary>
public sealed class SerializerOptions
{
    /// <summary>
    ///     Specific type serializer registrations.
    /// </summary>
    public HashSet<Type> Registrations { get; } = new();

    /// <summary>
    ///     Format serializer instance factory used for serialization under the scope.
    /// </summary>
    public InstanceFactory<IAbstractSerializer, Type>? FormatSerializerFactory { get; internal set; }

    /// <summary>
    ///     Determine if any serializing type is allowed despite configured <see cref="Registrations"/>.
    /// </summary>
    public bool IsAnyTypeAllowed { get; internal set; }
}
