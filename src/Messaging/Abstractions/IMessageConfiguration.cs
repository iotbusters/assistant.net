using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Common messaging configuration abstraction.
    ///     It simplifies configuring groups of related messages and helps avoiding code duplications.
    /// </summary>
    public interface IMessageConfiguration
    {
        /// <summary>
        ///     Configures <see cref="IMessagingClient"/>.
        /// </summary>
        void Configure(MessagingClientBuilder builder);
    }
}