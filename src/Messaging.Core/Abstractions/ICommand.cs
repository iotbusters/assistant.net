namespace Assistant.Net.Messaging.Abstractions
{
    public interface ICommand<out TResponse> : IAbstractCommand
    {
    }

    public interface ICommand : ICommand<None>
    {
    }

    public interface IAbstractCommand
    {
    }

    public sealed class None
    {
        internal static None Instance { get; } = new None();
    }
}