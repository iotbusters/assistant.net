using Assistant.Net.Messaging.Configuration;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface ICommandHandlerConfiguration
    {
        void Configure(CommandConfigurationBuilder builder);
    }
}