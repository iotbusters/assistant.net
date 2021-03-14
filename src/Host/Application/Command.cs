using System;
using System.Threading.Tasks;
using Assistant.Net.Messaging;

namespace Assistant.Net.Host.Application
{
    public class Command : IRequest
    {
        public async Task Invoke()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}