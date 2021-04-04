using Assistant.Net.Messaging.Options;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface ICommandConfiguration
    {
        void Configure(CommandOptions options);
    }
}