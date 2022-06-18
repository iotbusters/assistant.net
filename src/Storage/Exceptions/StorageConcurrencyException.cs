using System;

namespace Assistant.Net.Storage.Exceptions;

/// <summary>
///     Storage concurrency error occurred when optimistic concurrency write retries reached max number.
/// </summary>
public sealed class StorageConcurrencyException : StorageException
{
    /// <summary/>
    public StorageConcurrencyException() : this("Storage failed to write database concurrently.") { }

    /// <summary/>
    public StorageConcurrencyException(string? message) : base(message) { }

    /// <summary/>
    public StorageConcurrencyException(string? message, Exception? ex) : base(message, ex) { }
}
