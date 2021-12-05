namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Named messaging client factory abstraction.
    /// </summary>
    public interface IMessagingClientFactory
    {
        /// <summary>
        ///     Creates a default messaging client.
        /// </summary>
        IMessagingClient Create();

        /// <summary>
        ///     Creates a named messaging client.
        /// </summary>
        /// <param name="name">Client configuration name.</param>
        IMessagingClient Create(string name);
    }
}
