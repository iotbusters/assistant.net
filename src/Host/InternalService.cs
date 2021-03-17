using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assistant.Net.Host.Application;
using Assistant.Net.Messaging;

namespace Assistant.Net.Host
{
    public class InternalService
    {
        private readonly IRequestClient client;

        public InternalService(IRequestClient client) =>
            this.client = client;

        public async Task Handle1()
        {
            await client.Send(new Command2());
            var response = await client.Send(new Command1());
            await client.Send(new Event(response));
        }
    }
}