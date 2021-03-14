using System.Threading.Tasks;

namespace Assistant.Net.Messaging
{
    public interface IRequest<TResponse>
    {
        Task<TResponse> Invoke();
    }


    public interface IRequest
    {
        Task Invoke();
    }
}