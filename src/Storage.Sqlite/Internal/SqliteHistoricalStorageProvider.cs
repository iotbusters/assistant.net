using Assistant.Net.Abstractions;
using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

internal class SqliteHistoricalStorageProvider<TValue> : IHistoricalStorageProvider<TValue>
{
    private readonly ILogger logger;
    private readonly SqliteStoringOptions options;
    private readonly IDbContextFactory<StorageDbContext> dbContextFactory;

    public SqliteHistoricalStorageProvider(
        ILogger<SqliteHistoricalStorageProvider<TValue>> logger,
        INamedOptions<SqliteStoringOptions> options,
        IDbContextFactory<StorageDbContext> dbContextFactory)
    {
        this.logger = logger;
        this.options = options.Value;
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<ValueRecord> AddOrGet(
        KeyRecord key,
        Func<KeyRecord, Task<ValueRecord>> addFactory,
        CancellationToken token)
    {
        var strategy = options.InsertRetry;

        var attempt = 1;
        while (true)
        {
            logger.LogDebug("Storage.AddOrGet({KeyId}): {Attempt} begins.", key.Id, attempt);

            if (await TryGet(key, token) is Some<ValueRecord>(var found))
            {
                logger.LogDebug("Storage.AddOrGet({KeyId}:{Version}): {Attempt} found.",
                    key.Id, found.Audit.Version, attempt);
                return found;
            }

            var newValue = await addFactory(key);
            if (await InsertValue(key, newValue, token))
            {
                logger.LogDebug("Storage.AddOrGet({KeyId}:1): {Attempt} succeeded.", key.Id, attempt);
                return newValue;
            }

            logger.LogDebug("Storage.AddOrGet({KeyId}:1): {Attempt} failed.", key.Id, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogDebug("Storage.AddOrGet({KeyId}): {Attempt} won't proceed.", key.Id, attempt);
                break;
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        throw new StorageConcurrencyException();
    }

    public async Task<ValueRecord> AddOrUpdate(
        KeyRecord key,
        Func<KeyRecord, Task<ValueRecord>> addFactory,
        Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
        CancellationToken token)
    {
        var strategy = options.UpsertRetry;

        var attempt = 1;
        while (true)
        {
            logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} begins.", key.Id, attempt);

            var newValue = await TryGet(key, token) is Some<ValueRecord>(var found)
                ? await updateFactory(key, found)
                : await addFactory(key);
            if (await InsertValue(key, newValue, token))
            {
                logger.LogDebug("Storage.AddOrUpdate({KeyId}:{Version}): {Attempt} succeeded.",
                    key.Id, newValue.Audit.Version, attempt);
                return newValue;
            }

            logger.LogDebug("Storage.AddOrUpdate({KeyId}:{Version}): {Attempt} failed.",
                key.Id, newValue.Audit.Version, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} won't proceed.", key.Id, attempt);
                break;
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        throw new StorageConcurrencyException();
    }

    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}:latest) querying: begins.", key.Id);

        var storageDbContext = CreateDbContext();

        var found = await storageDbContext.HistoricalValues
            .AsNoTracking()
            .OrderBy(x => x.Version)
            .LastOrDefaultAsync(x => x.KeyId == key.Id, token);

        if (found != null)
            logger.LogDebug("SQLite({KeyId}:{Version}) querying: found.", key.Id, found.Version);
        else
            logger.LogDebug("SQLite({KeyId}:latest) querying: not found.", key.Id);

        return found.AsOption().MapOption(x =>
            new ValueRecord(x.ValueType, x.ValueContent, new Audit(x.Details.FromDetailArray(), x.Version)));
    }

    /// <exception cref="ArgumentOutOfRangeException"/>
    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}:{Version}) querying: begins.", key.Id, version);

        var storageDbContext = CreateDbContext();

        var found = await storageDbContext.HistoricalValues
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.KeyId == key.Id && x.Version == version, token); 

        if (found != null)
            logger.LogDebug("SQLite({KeyId}:{Version}) querying: found.", key.Id, found.Version);
        else
            logger.LogDebug("SQLite({KeyId}:{Version}) querying: not found.", key.Id, version);

        return found.AsOption().MapOption(x =>
            new ValueRecord(x.ValueType, x.ValueContent, new Audit(x.Details.FromDetailArray(), x.Version)));
    }

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token)
    {
        var strategy = options.DeleteRetry;

        var attempt = 1;
        while (true)
        {
            logger.LogDebug("Storage.TryRemove({KeyId}): {Attempt} begins.", key.Id, attempt);

            if (await DeleteMany(key, token) is not Some<ValueRecord>(var deleted))
            {
                logger.LogDebug("Storage.TryRemove({KeyId}): {Attempt} not found.", key.Id, attempt);
                return Option.None; // note: no value versions.
            }

            if (await TryGet(key, token) is not Some<ValueRecord>(var found)
                || deleted.Audit.Version < found.Audit.Version)
            {
                // note: 1) no new value version was added concurrently;
                //       2) new value version sequence was started.

                await CleanupKeys(token);

                logger.LogDebug("Storage.TryRemove({KeyId}): {Attempt}  succeeded.", key.Id, attempt);
                return deleted.AsOption(); 
            }

            logger.LogDebug("Storage.TryRemove({KeyId}): {Attempt} failed.", key.Id, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogDebug("Storage.TryRemove({KeyId}): {Attempt} won't proceed.", key.Id, attempt);
                break;
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        throw new StorageConcurrencyException();
    }

    public async Task<long> TryRemove(KeyRecord key, long upToVersion, CancellationToken token)
    {
        if (upToVersion <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {upToVersion}.");

        logger.LogDebug("Storage.TryRemove({KeyId}, 1..{Version}): begins.", key.Id, upToVersion);

        var deletedCount = await DeleteMany(key, upToVersion, token);

        await CleanupKeys(token);

        logger.LogDebug("Storage.TryRemove({KeyId}, 1..{Version}): succeeded with {Count}.", key.Id, upToVersion, deletedCount);
        return deletedCount;
    }

    public IQueryable<KeyRecord> GetKeys() =>
        CreateDbContext().HistoricalKeys.AsNoTracking().Select(x => new KeyRecord(x.Id, x.Type, x.Content, x.ValueType));

    public void Dispose() { }

    private StorageDbContext CreateDbContext() => dbContextFactory.CreateDbContext();

    private async Task<bool> InsertValue(KeyRecord key, ValueRecord newValue, CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}) inserting: begins.", key.Id);

        var dbContext = CreateDbContext();

        if (await dbContext.HistoricalKeys.AnyAsync(x => x.Id == key.Id, token))
            logger.LogDebug("SQLite({KeyId}) key: found.", key.Id);
        else
        {
            var keyRecord = new HistoricalKeyRecord(key.Id, key.Type, key.Content, key.ValueType);
            await dbContext.AddAsync(keyRecord, token);

            logger.LogDebug("SQLite({KeyId}) key: adding.", key.Id);
        }

        var valueRecord = new HistoricalValueRecord(
            key.Id,
            newValue.Type,
            newValue.Content,
            newValue.Audit.Version,
            newValue.Audit.Details.ToDetailArray());

        try
        {
            await dbContext.AddAsync(valueRecord, token);
            await dbContext.SaveChangesAsync(token);

            logger.LogDebug("SQLite({KeyId}) inserting: succeeded.", key.Id);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 1555})
        {
            // note: primary key constraint violation (https://www.sqlite.org/rescode.html#constraint_primarykey)
            logger.LogWarning(ex, "SQLite({KeyId}) inserting: already present.", key.Id);
            return false;
        }
    }

    private async Task<Option<ValueRecord>> DeleteMany(KeyRecord key, CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}) deleting: begins.", key.Id);

        if (await TryGet(key, token) is not Some<ValueRecord>(var found))
            return Option.None;

        var dbContext = CreateDbContext();
        var valueQuery = dbContext.HistoricalValues.Where(x => x.KeyId == key.Id && x.Version <= found.Audit.Version);

        var count = await valueQuery.LongCountAsync(token);
        logger.LogDebug("SQLite({KeyId}) deleting: found {Count}.", key.Id, count);

        dbContext.RemoveRange(valueQuery);
        await dbContext.SaveChangesAsync(token);

        logger.LogDebug("SQLite({KeyId}) deleting: succeeded.", key.Id);
        return found.AsOption();
    }

    private async Task<long> DeleteMany(KeyRecord key, long upToVersion, CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}) deleting: begins.", key.Id);

        var dbContext = CreateDbContext();
        var valueQuery = dbContext.HistoricalValues.Where(x => x.KeyId == key.Id && x.Version <= upToVersion);

        var count = await valueQuery.LongCountAsync(token);
        logger.LogDebug("SQLite({KeyId}) deleting: found {Count}.", key.Id, count);

        dbContext.RemoveRange(valueQuery);
        await dbContext.SaveChangesAsync(token);

        logger.LogDebug("SQLite({KeyId}) deleting: succeeded.", key.Id);
        return count;
    }

    private async Task CleanupKeys(CancellationToken token)
    {
        logger.LogDebug("SQLite(*) deleting: key cleanup begins.");

        var dbContext = CreateDbContext();
        var keys = dbContext.HistoricalKeys;
        var values = dbContext.HistoricalValues;
        var keyQuery = keys.Where(key => !values.Any(x => x.KeyId == key.Id));

        var count = await keyQuery.LongCountAsync(token);
        logger.LogDebug("SQLite(*) deleting: found {Count}.", count);

        dbContext.RemoveRange(keyQuery);
        await dbContext.SaveChangesAsync(token);

        logger.LogDebug("SQLite(*) deleting: key cleanup succeeded.");
    }
}
