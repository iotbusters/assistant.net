using Assistant.Net.Messaging;

namespace Assistant.Net.Host.Application
{
    public class Event : IRequest
    {
        private readonly Response response;
        public Event(Response response) =>
            this.response = response;
    }
}