using System;

namespace Assistant.Net.Storage.Exceptions;

/// <summary>
///     The exception thrown if no specific storage provider is configured.
/// </summary>
public class StorageProviderNotRegisteredException : StorageException
{
    /// <summary />
    public StorageProviderNotRegisteredException() : this("Storage provider wasn't properly registered.") { }

    /// <summary />
    public StorageProviderNotRegisteredException(string message) : base(message) { }

    /// <summary />
    public StorageProviderNotRegisteredException(string message, Exception? ex) : base(message, ex) { }
}
