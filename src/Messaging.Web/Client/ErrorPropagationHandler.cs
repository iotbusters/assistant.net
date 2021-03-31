using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Assistant.Net.Core;
using Assistant.Net.Messaging.Exceptions;
using Microsoft.Extensions.Options;

namespace Assistant.Net.Messaging.Web.Client
{
    public class ErrorPropagationHandler : DelegatingHandler
    {
        private readonly IOptions<JsonSerializerOptions> options;

        public ErrorPropagationHandler(IOptions<JsonSerializerOptions> options) =>
            this.options = options;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
                return response;

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var error = await JsonSerializer.DeserializeAsync<Exception>(stream, options.Value, cancellationToken);
            throw error ?? new NoneCommandException($"Response failed with status code: {(int)response.StatusCode}.");
        }
    }
}