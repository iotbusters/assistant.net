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
public sealed class ErrorPropagationHandler : DelegatingHandler
{
    private readonly ISerializer<MessageException> serializer;

    /// <summary/>
    public ErrorPropagationHandler(ISerializer<MessageException> serializer) =>
        this.serializer = serializer;

    /// <inheritdoc/>
    /// <exception cref="MessageDeferredException"/>
    /// <exception cref="MessageContractException"/>
    /// <exception cref="MessageException"/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    {
        var response = await base.SendAsync(request, token);

        var str = await response.Content.ReadAsStringAsync(token);

        return (response.IsSuccessStatusCode, response.StatusCode) switch
        {
            (true, HttpStatusCode.NotModified)  => response,
            (true, HttpStatusCode.OK)           => response,
            (true, HttpStatusCode.Accepted)     => throw new MessageDeferredException("Web message handler deferred a message."),
            (true, var x)                       => throw new MessageContractException(InvalidStatusMessage(x)),

            (false, HttpStatusCode.NotFound)            => throw await ReadException(response, token),
            (false, HttpStatusCode.InternalServerError) => throw await ReadException(response, token),
            (false, HttpStatusCode.BadGateway)          => throw await ReadException(response, token),
            (false, HttpStatusCode.FailedDependency)    => throw await ReadException(response, token),
            (false, HttpStatusCode.Forbidden)           => throw await ReadException(response, token),
            (false, HttpStatusCode.RequestTimeout)      => throw await ReadException(response, token),
            (false, HttpStatusCode.ServiceUnavailable)  => throw await ReadException(response, token),
            (false, HttpStatusCode.Unauthorized)        => throw await ReadException(response, token),
            (false, var x)                              => throw new MessageContractException(InvalidStatusMessage(x)),
        };
    }

    /// <exception cref="MessageContractException"/>
    private async Task<MessageException> ReadException(HttpResponseMessage response, CancellationToken token)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(token);
        try
        {
            return await serializer.Deserialize(stream, token);
        }
        catch (Exception ex)
        {
            return new MessageContractException(ErrorContentMessage(response.StatusCode), ex);
        }
    }

    private static string InvalidStatusMessage(HttpStatusCode status) =>
        $"Response returned unexpected status code: {(int)status}.";
    private static string ErrorContentMessage(HttpStatusCode status) =>
        $"Response returned unexpected content for the status code: {(int)status}.";
}
