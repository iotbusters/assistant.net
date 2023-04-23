using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Web.Tests.Mocks;

public class TestDelegatingHandler : DelegatingHandler
{
    private readonly HttpStatusCode status;
    private readonly byte[] response;

    public TestDelegatingHandler(byte[] response, HttpStatusCode status)
    {
        this.status = status;
        this.response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Request = request;
        return Task.FromResult(new HttpResponseMessage(status) {Content = new ByteArrayContent(response)});
    }

    public HttpRequestMessage? Request { get; private set; } = null!;
}
