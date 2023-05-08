using Assistant.Net.Abstractions;
using Assistant.Net.Messaging.Abstractions;
using Assistant.Net.Messaging.Exceptions;
using Assistant.Net.Messaging.Models;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Unions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

/// <summary>
///     Round-robin implementation of a host selection strategy which spreads hosts evenly between requests.
/// </summary>
internal sealed class RoundRobinSelectionStrategy : IHostSelectionStrategy
{
    private readonly ILogger<RoundRobinSelectionStrategy> logger;
    private readonly IStorage<string, HostsAvailabilityModel> hostRegistrationStorage;
    private readonly ITypeEncoder typeEncoder;
    private readonly ISystemClock clock;

    private HostsAvailabilityModel? configuration;
    private int indexer;

    public RoundRobinSelectionStrategy(
        ILogger<RoundRobinSelectionStrategy> logger,
        IStorage<string, HostsAvailabilityModel> hostRegistrationStorage,
        ITypeEncoder typeEncoder,
        ISystemClock clock)
    {
        this.logger = logger;
        this.hostRegistrationStorage = hostRegistrationStorage;
        this.typeEncoder = typeEncoder;
        this.clock = clock;
    }

    /// <inheritdoc/>
    /// <exception cref="MessageNotRegisteredException"/>
    public async Task<string?> GetInstance(Type messageType, CancellationToken token)
    {
        logger.LogDebug("Host selection: begins.");

        configuration ??= await GetLatestConfiguration(token);

        var messageName = typeEncoder.Encode(messageType)!;
        var registration = SelectInstance(configuration.Registrations.Where(x => x.Messages.Contains(messageName)).ToArray())
                           ?? SelectInstance(configuration.Registrations.Where(x => x.AcceptOthers).ToArray());
        var now = clock.UtcNow;

        if (registration == null || registration.Expired <= now)
        {
            configuration = await GetLatestConfiguration(token);

            registration = SelectInstance(configuration.Registrations.Where(x => x.Messages.Contains(messageName)).ToArray())
                           ?? SelectInstance(configuration.Registrations.Where(x => x.AcceptOthers).ToArray());
        }

        if (registration == null)
            logger.LogWarning("Host selection: ends without a host.");
        else
            logger.LogInformation("Host selection: ends with {host} to be expired in {ExpirationTime}.",
                registration.Instance,
                registration.Expired - now);

        return registration?.Instance;
    }

    private async Task<HostsAvailabilityModel> GetLatestConfiguration(CancellationToken token)
    {
        logger.LogDebug("Host reload: begins.");

        if (await hostRegistrationStorage.TryGet(HostsAvailabilityModel.Key, token) is not Some<HostsAvailabilityModel>(var model))
            throw new MessageNotRegisteredException("No host was found.");

        var now = clock.UtcNow;

        foreach (var registration in model.Registrations)
            if (registration.Expired <= now)
                logger.LogWarning("Host reload: loaded {host} instance has expired.", registration.Instance);
            else
                logger.LogDebug("Host reload: loaded {host} instance expires in {ExpirationTime}.",
                    registration.Instance,
                    registration.Expired - now);

        logger.LogInformation("Host reload: ends.");

        return model.Remove(now);
    }

    private HostRegistrationModel? SelectInstance(IReadOnlyList<HostRegistrationModel> instances) => instances.Count switch
    {
        0 => null,
        1 => instances[0],
        var l => instances[Interlocked.Increment(ref indexer) % l]
    };
}
