using System;
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
    /// <summary>
    ///     Remote command handling failure propagation.
    /// </summary>
    internal class ErrorPropagationHandler : DelegatingHandler
    {
        private readonly IOptions<JsonSerializerOptions> options;

        public ErrorPropagationHandler(IOptions<JsonSerializerOptions> options) =>
            this.options = options;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            return (response.IsSuccessStatusCode, response.StatusCode) switch
            {
                (true, HttpStatusCode.NotModified)  => response,
                (true, HttpStatusCode.OK)           => response,
                (true, HttpStatusCode.Accepted)     => throw new CommandDeferredException(),
                (true, var x)                       => throw new CommandContractException(InvalidStatusMessage(x)),

                (false, HttpStatusCode.NotFound)            => throw await ReadException(response, cancellationToken),
                (false, HttpStatusCode.InternalServerError) => throw await ReadException(response, cancellationToken),
                (false, HttpStatusCode.BadGateway)          => throw await ReadException(response, cancellationToken),
                (false, HttpStatusCode.FailedDependency)    => throw await ReadException(response, cancellationToken),
                (false, HttpStatusCode.Forbidden)           => throw await ReadException(response, cancellationToken),
                (false, HttpStatusCode.RequestTimeout)      => throw await ReadException(response, cancellationToken),
                (false, HttpStatusCode.ServiceUnavailable)  => throw await ReadException(response, cancellationToken),
                (false, HttpStatusCode.Unauthorized)        => throw await ReadException(response, cancellationToken),
                (false, var x)                              => throw new CommandContractException(InvalidStatusMessage(x)),
            };
        }

        private async Task<Exception> ReadException(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await stream.ReadException(options.Value, cancellationToken)
            ?? new CommandContractException(ErrorContentMessage(response.StatusCode));
        }

        private static string InvalidStatusMessage(HttpStatusCode status) =>
            $"Response returned unexpected status code: {(int)status}.";
        private static string ErrorContentMessage(HttpStatusCode status) =>
            $"Response returned unexpected content for the status code: {(int)status}.";
    }
}