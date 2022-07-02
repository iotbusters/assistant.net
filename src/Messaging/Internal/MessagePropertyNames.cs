using Assistant.Net.Storage.Internal;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Storage audit property names for messaging.
/// </summary>
public static class MessagePropertyNames
{
    /// <summary>
    ///     Message handling request ID property name.
    /// </summary>
    public const string RequestIdName = "requestId";

    /// <inheritdoc cref="StoragePropertyNames.CorrelationIdName"/>
    public const string CorrelationIdName = StoragePropertyNames.CorrelationIdName;

    /// <inheritdoc cref="StoragePropertyNames.UserName"/>
    public const string UserName = StoragePropertyNames.UserName;

    /// <inheritdoc cref="StoragePropertyNames.CreatedName"/>
    public const string CreatedName = StoragePropertyNames.CreatedName;
}
