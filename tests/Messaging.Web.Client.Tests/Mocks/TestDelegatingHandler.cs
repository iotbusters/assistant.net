using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Client.Tests.Mocks
{
    public class TestDelegatingHandler : DelegatingHandler
    {
        private readonly HttpStatusCode status;
        private readonly object response;

        public TestDelegatingHandler(object response)
        {
            this.status = HttpStatusCode.OK;
            this.response = response;
        }

        public TestDelegatingHandler(Exception exception)
        {
            this.status = HttpStatusCode.InternalServerError;
            this.response = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = JsonContent.Create(response)
            });
        }

        public HttpRequestMessage? Request { get; private set; } = null!;
    }
}