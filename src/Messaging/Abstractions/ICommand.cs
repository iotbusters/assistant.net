namespace Assistant.Net.Messaging.Abstractions
{
    /// <summary>
    ///     Command abstraction that expects <typeparamref name="TResponse" /> object in response to the command request.
    /// </summary>
    public interface ICommand<out TResponse> : IAbstractCommand
    {
    }

    /// <summary>
    ///     Command abstraction that doesn't expect 
    /// </summary>
    public interface ICommand : ICommand<None>
    {
    }

    /// <summary>
    ///     Very generic command abstraction that is used primarily for type restrictions 
    ///     in configuration and other internal logic.
    /// </summary>
    public interface IAbstractCommand
    {
    }

    /// <summary>
    ///     No command response representation.
    /// </summary>
    public sealed class None
    {
        internal static None Instance { get; } = new None();
    }
}