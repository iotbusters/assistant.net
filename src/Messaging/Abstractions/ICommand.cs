namespace Assistant.Net.Messaging.Abstractions
{
    public interface ICommand<out TResponse>
    {
    }

    public interface ICommand : ICommand<None>
    {
    }

    public sealed class None
    {
        internal static None Instance { get; } = new None();
    }
}