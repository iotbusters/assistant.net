using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface IRemoteCommandClient
    {
         /// <summary>
        ///     Delegates <paramref name="command"/> handling to remote handler.
        /// </summary>
        Task<TResponse> DelegateHandling<TResponse>(ICommand<TResponse> command);
    }
}