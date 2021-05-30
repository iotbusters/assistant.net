using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Common command configuration object abstraction.
    ///     It simplifies configuring groups of related commands and helps avoiding code duplications.
    /// </summary>
    public interface ICommandConfiguration
    {
        /// <summary>
        ///     Configures command client.
        /// </summary>
        void Configure(CommandClientBuilder builder);
    }
}