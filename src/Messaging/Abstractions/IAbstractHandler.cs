using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Abstractions
{
    public interface IAbstractHandler
    {
        Task<object> Handle(object command);
    }
}