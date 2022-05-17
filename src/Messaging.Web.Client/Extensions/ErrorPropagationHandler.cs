using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Serialization.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Extensions;

/// <summary>
///     Remote message handling failure propagation.
/// </summary>
public class ErrorPropagationHandler : DelegatingHandler
{
    private readonly ISerializer<MessageException> serializer;

    /// <summary/>
    public ErrorPropagationHandler(ISerializer<MessageException> serializer) =>
        this.serializer = serializer;

    /// <inheritdoc/>
    /// <exception cref="MessageDeferredException"/>
    /// <exception cref="MessageContractException"/>
    /// <exception cref="MessageException"/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        return (response.IsSuccessStatusCode, response.StatusCode) switch
        {
            (true, HttpStatusCode.NotModified)  => response,
            (true, HttpStatusCode.OK)           => response,
            (true, HttpStatusCode.Accepted)     => throw new MessageDeferredException(),
            (true, var x)                       => throw new MessageContractException(InvalidStatusMessage(x)),

            (false, HttpStatusCode.NotFound)            => throw await ReadException(response, cancellationToken),
            (false, HttpStatusCode.InternalServerError) => throw await ReadException(response, cancellationToken),
            (false, HttpStatusCode.BadGateway)          => throw await ReadException(response, cancellationToken),
            (false, HttpStatusCode.FailedDependency)    => throw await ReadException(response, cancellationToken),
            (false, HttpStatusCode.Forbidden)           => throw await ReadException(response, cancellationToken),
            (false, HttpStatusCode.RequestTimeout)      => throw await ReadException(response, cancellationToken),
            (false, HttpStatusCode.ServiceUnavailable)  => throw await ReadException(response, cancellationToken),
            (false, HttpStatusCode.Unauthorized)        => throw await ReadException(response, cancellationToken),
            (false, var x)                              => throw new MessageContractException(InvalidStatusMessage(x)),
        };
    }

    /// <exception cref="MessageContractException"/>
    private async Task<MessageException> ReadException(HttpResponseMessage response, CancellationToken token)
    {
        var stream = await response.Content.ReadAsStreamAsync(token);
        try
        {
            return await serializer.Deserialize(stream, token);
        }
        catch (Exception e)
        {
            return new MessageContractException(ErrorContentMessage(response.StatusCode), e);
        }
    }

    private static string InvalidStatusMessage(HttpStatusCode status) =>
        $"Response returned unexpected status code: {(int)status}.";
    private static string ErrorContentMessage(HttpStatusCode status) =>
        $"Response returned unexpected content for the status code: {(int)status}.";
}
