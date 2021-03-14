using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging;

namespace Assistant.Net.Host.Application
{
    public class Request : IRequest<Response>
    {
        public async Task<Response> Invoke()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            return new Response();
        }
    }
}