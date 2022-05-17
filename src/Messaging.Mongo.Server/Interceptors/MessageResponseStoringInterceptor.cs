using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Interceptors;

/// <summary>
///     Message response storing interceptor, suddenly.
/// </summary>
public class MessageResponseStoringInterceptor : IMessageInterceptor
{
    private readonly ILogger<MessageResponseStoringInterceptor> logger;
    private readonly IStorage<IAbstractMessage, CachingResult> responseStorage;
    private readonly ITypeEncoder typeEncoder;

    /// <summary/>
    public MessageResponseStoringInterceptor(
        ILogger<MessageResponseStoringInterceptor> logger,
        IStorage<IAbstractMessage, CachingResult> responseStorage,
        ITypeEncoder typeEncoder)
    {
        this.logger = logger;
        this.responseStorage = responseStorage;
        this.typeEncoder = typeEncoder;
    }

    /// <inheritdoc/>
    public async Task<object> Intercept(Func<IMessage<object>, CancellationToken, Task<object>> next, IMessage<object> message, CancellationToken token)
    {
        var messageName = typeEncoder.Encode(message.GetType())
                          ?? throw new NotSupportedException($"Not supported  message type '{message.GetType()}'.");
        var messageId = message.GetSha1();

        var result = await responseStorage.AddOrGet(message, async _ =>
        {
            CachingResult result;
            try
            {
                var response = await next(message, token);
                result = CachingResult.OfValue((dynamic)response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message({MessageName}/{MessageId}) handling has failed.", messageName, messageId);
                result = CachingResult.OfException(ex);
            }

            logger.LogDebug("Message({MessageName}/{MessageId}) handling has succeeded.", messageName, messageId);
            return result;
        }, token);

        return result.GetValue();
    }
}
