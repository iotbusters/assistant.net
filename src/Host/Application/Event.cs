using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging;

namespace Assistant.Net.Host.Application
{
    public class Event : IRequest
    {
        private readonly Promise<Response> responsePromise;
        public Event(Promise<Response> responsePromise)
        {
            this.responsePromise = responsePromise;

        }

        public async Task Invoke()
        {
            await responsePromise.WaitResponse();

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}