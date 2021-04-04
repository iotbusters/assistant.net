using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Serialization;

namespace Assistant.Net.Messaging.Extensions
{
    public class ErrorPropagationHandler : DelegatingHandler
    {
        private readonly IOptions<JsonSerializerOptions> options;

        public ErrorPropagationHandler(IOptions<JsonSerializerOptions> options) =>
            this.options = options;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                if (SucceededStatuses.Contains(response.StatusCode))
                    return response;
                else
                    throw new CommandContractException(InvalidStatusMessage(response.StatusCode));

            if (ConnectionErrorStatuses.Contains(response.StatusCode))
                throw new CommandConnectionFailedException(ErrorMessage(response.StatusCode));

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            throw await stream.ReadException(options.Value, cancellationToken)
            ?? new CommandContractException(ErrorContentMessage(response.StatusCode));
        }

        private static HttpStatusCode[] SucceededStatuses { get; } = new[]
        {
            HttpStatusCode.NotModified,
            HttpStatusCode.OK
        };

        private static HttpStatusCode[] ConnectionErrorStatuses { get; } = new[]
        {
            HttpStatusCode.BadGateway,
            HttpStatusCode.FailedDependency,
            HttpStatusCode.Forbidden,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.Unauthorized
        };

        private static string InvalidStatusMessage(HttpStatusCode status) =>
            $"Response invalid success status code: {(int)status}.";
        private static string ErrorMessage(HttpStatusCode status) =>
            $"Response failed with status code: {(int)status}.";
        private static string ErrorContentMessage(HttpStatusCode status) =>
            $"Response failed with status code: {(int)status} and provided invalid content.";
    }
}