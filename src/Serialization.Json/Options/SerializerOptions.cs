using Assistant.Net.Options;
using Assistant.Net.Serialization.Abstractions;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Serialization.Options;

/// <summary>
///     Serializer configuration.
/// </summary>
public class SerializerOptions
{
    /// <summary>
    ///     Specific type serializer factories.
    /// </summary>
    public Dictionary<Type, InstanceFactory<IAbstractSerializer>> Registrations { get; } = new();

    /// <summary>
    ///     Any other type (except explicitly registered in <see cref="Registrations"/>) serializer factory.
    /// </summary>
    public InstanceFactory<IAbstractSerializer, Type>? AnyTypeRegistration { get; set; }
}
