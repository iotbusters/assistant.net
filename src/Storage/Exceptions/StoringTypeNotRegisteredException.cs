using System;

namespace Assistant.Net.Storage.Exceptions;

/// <summary>
///     The exception thrown if specific storing type isn't configured.
/// </summary>
public class StoringTypeNotRegisteredException : StorageException
{
    /// <summary />
    public StoringTypeNotRegisteredException(Type storingType) : this($"Storage({storingType}) wasn't registered.") { }

    /// <summary />
    public StoringTypeNotRegisteredException(string message) : base(message) { }

    /// <summary />
    public StoringTypeNotRegisteredException(string message, Exception? ex) : base(message, ex) { }
}
