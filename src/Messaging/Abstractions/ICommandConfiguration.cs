using Assistant.Net.Messaging.Configuration;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface ICommandConfiguration
    {
        void Configure(CommandConfigurationBuilder builder);
    }
}