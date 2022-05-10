using System;

namespace Assistant.Net.Storage.Exceptions;

/// <summary>
///     Generic storage exception.
/// </summary>
public class StorageException : Exception
{
    private const string DefaultMessage = "Storage operation has failed.";

    /// <summary />
    public StorageException() : this(DefaultMessage) { }

    /// <summary />
    public StorageException(string? message) : base(message ?? DefaultMessage) { }

    /// <summary />
    public StorageException(string? message, Exception? ex) : base(message ?? DefaultMessage, ex) { }

    /// <summary />
    public StorageException(Exception ex) : base(DefaultMessage, ex) { }
}
