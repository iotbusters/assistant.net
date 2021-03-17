using Assistant.Net.Messaging;

namespace Assistant.Net.Host.Application
{
    public class Command1 : IRequest<Response>
    {
        public Command1()
        {
        }
    }

    public class Command2 : IRequest
    {
        public Command2()
        {
        }
    }
}