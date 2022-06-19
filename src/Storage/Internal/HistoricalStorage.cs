using Assistant.Net.Abstractions;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

internal class HistoricalStorage<TKey, TValue> : Storage<TKey, TValue>, IHistoricalAdminStorage<TKey, TValue>
{
    private readonly IHistoricalStorageProvider<TValue> backedStorage;

    /// <exception cref="ArgumentException"/>
    public HistoricalStorage(
        IServiceProvider provider,
        IDiagnosticContext diagnosticContext,
        ISystemClock clock,
        ITypeEncoder typeEncoder) : base(
        provider,
        diagnosticContext,
        clock,
        typeEncoder,
        GetProvider(provider)) =>
        backedStorage = (IHistoricalStorageProvider<TValue>)base.BackedStorage;

    public async Task<Option<TValue>> TryGet(TKey key, long version, CancellationToken token)
    {
        if (version <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {version}.", nameof(version));
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            return await backedStorage.TryGet(keyRecord, version, token).MapOption(x => ValueConverter.Convert(x.Content, token));
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    public async Task<long> TryRemove(TKey key, long upToVersion, CancellationToken token)
    {
        if (upToVersion <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {upToVersion}.", nameof(upToVersion));
        try
        {
            var keyRecord = await CreateKeyRecord(key, token);
            return await backedStorage.TryRemove(keyRecord, upToVersion, token);
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException(ex);
        }
    }

    private static IHistoricalStorageProvider<TValue> GetProvider(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<INamedOptions<StorageOptions>>().Value;
        return options.HistoricalProviders.TryGetValue(typeof(TValue), out var factory)
            ? (IHistoricalStorageProvider<TValue>)factory.Create(provider)
            : throw new ArgumentException($"HistoricalStorage({typeof(TValue).Name}) wasn't properly configured.");
    }
}
