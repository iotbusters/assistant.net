using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Common messaging configuration abstraction required for configuring message client in named groups
    ///     resolving code duplication issues and improving code readability.
    /// </summary>
    public interface IMessageConfiguration
    {
        /// <summary>
        ///     Configures <see cref="IMessagingClient"/>.
        /// </summary>
        void Configure(MessagingClientBuilder builder);
    }
}
