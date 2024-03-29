﻿using System;

namespace Assistant.Net.Serialization.Abstractions;

/// <summary>
///     An abstraction over serializer factory.
/// </summary>
public interface ISerializerFactory
{
    /// <summary>
    ///     Creates a de-typed serializer of <paramref name="serializingType"/>.
    /// </summary>
    IAbstractSerializer Create(Type serializingType);

    /// <summary>
    ///     Creates a de-typed serializer of <typeparamref name="T"/>.
    /// </summary>
    ISerializer<T> Create<T>();
}
