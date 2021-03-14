using System.Collections.Generic;
using Assistant.Net.Host.Application;
using Assistant.Net.Messaging;

namespace Assistant.Net.Host
{
    public class InternalService
    {
        private readonly IOperationClient client;

        public InternalService(IOperationClient client)
        {
            this.client = client;
        }

        public IEnumerable<Promise> Handle1()
        {
            yield return client.Send(new Command());
            var response = client.Send(new Request());
            yield return client.Send(new Event(response));
        }
    }
}