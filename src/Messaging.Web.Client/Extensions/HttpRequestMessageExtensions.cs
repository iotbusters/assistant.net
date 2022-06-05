using System;
using System.Linq;
using System.Net.Http;

namespace Assistant.Net.Messaging.Extensions;

/// <summary>
///     Common operation over <see cref="HttpRequestMessage"/> object.
/// </summary>
public static  class HttpRequestMessageExtensions
{
    /// <summary>
    ///     Gets a header value from the request and fail if it's missing.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public static string GetRequiredHeader(this HttpRequestMessage request, string name)
    {
        if (!request.Headers.TryGetValues(name, out var values) || !values.Any())
            throw new InvalidOperationException($"Header '{name}' is required.");

        return values.First();
    }

    /// <summary>
    ///     Adds a header to the request.
    /// </summary>
    public static void SetHeader(this HttpRequestMessage request, string name, string value) =>
        request.Headers.Add(name, value);

    /// <summary>
    ///     Gets a message name from the request headers.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public static string GetMessageName(this HttpRequestMessage request) =>
        request.GetRequiredHeader(ClientHeaderNames.MessageName);

    /// <summary>
    ///     Adds a correlation id to the request headers.
    /// </summary>
    public static void SetCorrelationId(this HttpRequestMessage request, string correlationId) =>
        request.SetHeader(ClientHeaderNames.CorrelationId, correlationId);
}
