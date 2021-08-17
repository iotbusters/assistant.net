using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Serialization.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Extensions
{
    /// <summary>
    ///     Remote command handling failure propagation.
    /// </summary>
    public class ErrorPropagationHandler : DelegatingHandler
    {
        private readonly ISerializer<CommandException> serializer;

        /// <summary/>
        public ErrorPropagationHandler(ISerializer<CommandException> serializer) =>
            this.serializer = serializer;

        /// <inheritdoc/>
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

        private async Task<CommandException> ReadException(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            try
            {
                return await serializer.Deserialize(stream);
            }
            catch (Exception e)
            {
                return new CommandContractException(ErrorContentMessage(response.StatusCode), e);
            }
        }

        private static string InvalidStatusMessage(HttpStatusCode status) =>
            $"Response returned unexpected status code: {(int)status}.";
        private static string ErrorContentMessage(HttpStatusCode status) =>
            $"Response returned unexpected content for the status code: {(int)status}.";
    }
}