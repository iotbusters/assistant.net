using System;

namespace Assistant.Net.Storage.Exceptions;

/// <summary>
///     An exception thrown if no specific value converter isn't configured.
/// </summary>
public class ConverterNotRegisteredException : StorageException
{
    /// <summary />
    public ConverterNotRegisteredException(Type convertingType) : this($"ValueConverter({convertingType}) wasn't registered.") { }

    /// <summary />
    public ConverterNotRegisteredException(string message) : base(message) { }

    /// <summary />
    public ConverterNotRegisteredException(string message, Exception? ex) : base(message, ex) { }
}
