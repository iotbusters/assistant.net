using System;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Command handler factory abstraction.
    /// </summary>
    public interface IHandlerFactory
    {
        /// <summary>
        ///     Creates new instance of command handler by <paramref name="commandType" />.
        /// </summary>
        IAbstractHandler Create(Type commandType);
    }
}