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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        var context = CreateDbContext();

        var keyId = new Key(key.Id, key.ValueType);
        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrGet({@Key}): {Attempt} begins.", keyId, attempt);

            if (await FindLatestOne(context, key, token) is Some<ValueRecord>(var found))
            {
                logger.LogInformation("Storage.AddOrGet({@Key}, {Version}): {Attempt} found.",
                    keyId, found.Audit.Version, attempt);
                return found;
            }

            var value = await addFactory(key);
            if (await InsertValue(context, key, value, token))
            {
                logger.LogInformation("Storage.AddOrGet({@Key}, {Version}): {Attempt} added.",
                    keyId, value.Audit.Version, attempt);
                return value;
            }

            logger.LogWarning("Storage.AddOrGet({@Key}): {Attempt} ends.", keyId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrGet({@Key}): {Attempt} reached the limit.", keyId, attempt);
                throw new StorageConcurrencyException();
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async Task<ValueRecord> AddOrUpdate(
        KeyRecord key,
        Func<KeyRecord, Task<ValueRecord>> addFactory,
        Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
        CancellationToken token)
    {
        var strategy = options.UpsertRetry;
        var context = CreateDbContext();

        var keyId = new Key(key.Id, key.ValueType);
        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrUpdate({@Key}): {Attempt} begins.", keyId, attempt);

            var value = await FindLatestOne(context, key, token) is Some<ValueRecord>(var found)
                ? await updateFactory(key, found)
                : await addFactory(key);
            if (await InsertValue(context, key, value, token))
            {
                logger.LogInformation("Storage.AddOrUpdate({@Key}, {Version}): {Attempt} added.",
                    keyId, value.Audit.Version, attempt);
                return value;
            }

            logger.LogWarning("Storage.AddOrUpdate({@Key}): {Attempt} ends.", keyId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrUpdate({@Key}): {Attempt} reached the limit.", keyId, attempt);
                throw new StorageConcurrencyException();
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogDebug("SQLite ({@Key}, latest) querying: begins.", keyId);

        var context = CreateDbContext();
        if (await FindLatestOne(context, key, token) is Some<ValueRecord>(var found) option)
        {
            logger.LogDebug("SQLite({@Key}, {Version}) querying: found.", keyId, found.Audit.Version);
            return option;
        }

        logger.LogDebug("SQLite ({@Key}, latest) querying: not found.", keyId);
        return Option.None;
    }

    /// <exception cref="ArgumentOutOfRangeException"/>
    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, long version, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogDebug("SQLite({@Key}, {Version}) querying: begins.", keyId, version);

        var context = CreateDbContext();
        if (await FindExactOne(context, key, version, token) is Some<ValueRecord>(var found) option)
        {
            logger.LogDebug("SQLite({@Key}, {Version}) querying: found.", keyId, found.Audit.Version);
            return option;
        }

        logger.LogDebug("SQLite({@Key}, {Version}) querying: not found.", keyId, version);
        return Option.None;
    }

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token)
    {
        var strategy = options.DeleteRetry;
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogInformation("Storage.TryRemove({@Key}, 1..latest): begins.", keyId);

        var context = CreateDbContext();
        ValueRecord? latestValue = null;
        var attempt = 1;
        while (true)
        {
            if (await FindLatestOne(context, key, token) is not Some<ValueRecord>(var found))
                break;

            if (latestValue != null && found.Audit.Version <= latestValue.Audit.Version)
                break; // note: found new value version sequence was started.

            latestValue = found;

            if (await DeleteMany(context, key, found.Audit.Version, token))
            {
                await CleanupKeys(context, token);
                break;
            }

            logger.LogWarning("Storage.TryRemove({@Key}, 1..latest): {Attempt} ends.", keyId, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.TryRemove({@Key}, 1..latest): {Attempt} reached the limit.", keyId, attempt);
                break;
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        if (latestValue != null)
        {
            logger.LogInformation("Storage.TryRemove({@Key}, 1..{Version}): succeeded.", keyId, latestValue.Audit.Version);
            return Option.Some(latestValue);
        }

        logger.LogInformation("Storage.TryRemove({@Key}, 1..latest): not found.", keyId);
        return Option.None; // note: value doesn't exist.
    }

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, long upToVersion, CancellationToken token)
    {
        if (upToVersion <= 0)
            throw new ArgumentOutOfRangeException($"Value must be bigger than 0 but it was {upToVersion}.");

        var strategy = options.DeleteRetry;
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogInformation("Storage.TryRemove({@Key}, 1..{Version}): begins.", keyId, upToVersion);

        var context = CreateDbContext();
        ValueRecord? latestValue = null;
        var attempt = 1;
        while (true)
        {
            if (await FindLatestOne(context, key, upToVersion, token) is not Some<ValueRecord>(var found))
                break;

            if (latestValue != null && found.Audit.Version <= latestValue.Audit.Version)
                break; // note: found new value version sequence was started.

            latestValue = found;

            if (await DeleteMany(context, key, found.Audit.Version, token))
            {
                await CleanupKeys(context, token);
                break;
            }

            logger.LogWarning("Storage.TryRemove({@Key}, 1..{Version}): {Attempt} ends.", keyId, upToVersion, attempt);

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.TryRemove({@Key}, 1..{Version}): {Attempt} reached the limit.", keyId, upToVersion, attempt);
                break;
            }

            await Task.Delay(strategy.DelayTime(attempt), token);
        }

        if (latestValue != null)
        {
            logger.LogInformation("Storage.TryRemove({@Key}, 1..{Version}): succeeded.", keyId, latestValue.Audit.Version);
            return Option.Some(latestValue);
        }

        logger.LogInformation("Storage.TryRemove({@Key}, 1..{Version}): not found.", keyId, upToVersion);
        return Option.None; // note: value doesn't exist.
    }

    public IAsyncEnumerable<KeyRecord> GetKeys(Expression<Func<KeyRecord, bool>> predicate, CancellationToken token) =>
        CreateDbContext().HistoricalKeys.AsNoTracking()
            .Select(x => new KeyRecord {Id = x.Id, Type = x.Type, Content = x.Content, ValueType = x.ValueType})
            .Where(predicate)
            .AsAsyncEnumerable();

    private StorageDbContext CreateDbContext() => dbContextFactory.CreateDbContext();

    private async Task<Option<ValueRecord>> FindLatestOne(StorageDbContext context, KeyRecord key, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogDebug("SQLite ({@Key}, latest) querying: begins.", keyId);

        var found = await context.HistoricalValues
            .AsNoTracking()
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(x => x.KeyId == key.Id && x.ValueType == key.ValueType, token);

        if (found != null)
        {
            logger.LogDebug("SQLite({@Key}, {Version}) querying: found.", keyId, found.Version);
            return ToValue(found).AsOption();
        }

        logger.LogDebug("SQLite ({@Key}, latest) querying: not found.", keyId);
        return Option.None;
    }

    private async Task<Option<ValueRecord>> FindLatestOne(StorageDbContext context, KeyRecord key, long upToVersion, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogDebug("SQLite ({@Key}, 1..{Version}) querying: begins.", keyId, upToVersion);

        var found = await context.HistoricalValues
            .AsNoTracking()
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(x => x.KeyId == key.Id && x.ValueType == key.ValueType && x.Version <= upToVersion, token);

        if (found != null)
        {
            logger.LogDebug("SQLite({@Key}, {Version}) querying: found.", keyId, found.Version);
            return ToValue(found).AsOption();
        }

        logger.LogDebug("SQLite ({@Key}, 1..{Version}) querying: not found.", keyId, upToVersion);
        return Option.None;
    }

    private async Task<Option<ValueRecord>> FindExactOne(StorageDbContext context, KeyRecord key, long version, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogDebug("SQLite({@Key}, {Version}) querying: begins.", keyId, version);

        var found = await context.HistoricalValues
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.KeyId == key.Id && x.ValueType == key.ValueType && x.Version == version, token);

        if (found != null)
        {
            logger.LogDebug("SQLite({@Key}, {Version}) querying: found.", keyId, found.Version);
            return ToValue(found).AsOption();
        }

        logger.LogDebug("SQLite({@Key}, {Version}) querying: not found.", keyId, version);
        return Option.None;
    }

    private static ValueRecord ToValue(HistoricalValueRecord record) =>
        new(record.ValueContent, new Audit(record.Details.FromDetailArray(), record.Version));

    private async Task<bool> InsertValue(StorageDbContext context, KeyRecord key, ValueRecord value, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogDebug("SQLite({@Key}) inserting: begins.", keyId);

        if (await context.HistoricalKeys.AnyAsync(x => x.Id == key.Id && x.ValueType == key.ValueType, token))
            logger.LogDebug("SQLite({@Key}) inserting: key found.", keyId);
        else
        {
            var keyRecord = new HistoricalKeyRecord(key.Id, key.Type, key.Content, key.ValueType);
            await context.AddAsync(keyRecord, token);

            logger.LogDebug("SQLite({@Key}) inserting: key adding.", keyId);
        }

        var inserted = new HistoricalValueRecord(
            key.Id,
            key.ValueType,
            value.Content,
            value.Audit.Version,
            value.Audit.Details.ToDetailArray());

        try
        {
            await context.AddAsync(inserted, token);
            await context.SaveChangesAsync(token);

            logger.LogDebug("SQLite({@Key}, {Version}) inserting: succeeded.", keyId, value.Audit.Version);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 1555})
        {
            // note: primary key constraint violation (https://www.sqlite.org/rescode.html#constraint_primarykey)
            logger.LogWarning(ex, "SQLite({@Key}, {Version}) inserting: already present.", keyId, value.Audit.Version);
            context.ChangeTracker.Clear();
            return false;
        }
    }

    private async Task<bool> DeleteMany(StorageDbContext context, KeyRecord key, long upToVersion, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogDebug("SQLite({@Key}, 1..{Version}) deleting: begins.", keyId, upToVersion);

        var values = await context.HistoricalValues
            .Where(x => x.KeyId == key.Id && x.ValueType == key.ValueType && x.Version <= upToVersion)
            .Select(x => new HistoricalValueRecord(key.Id, key.ValueType, null!, x.Version))
            .ToArrayAsync(token);

        logger.LogDebug("SQLite({@Key}, 1..{Version}) deleting: found {Count}.", keyId, upToVersion, values.Length);

        try
        {
            context.HistoricalValues.AttachRange(values);
            context.HistoricalValues.RemoveRange(values);
            await context.SaveChangesAsync(token);
        }
        catch (DbUpdateException ex)
        {
            logger.LogDebug(ex, "SQLite({@Key}, 1..{Version}) deleting: concurrently deleted.", keyId, upToVersion);
            context.ChangeTracker.Clear();
            return false;
        }

        logger.LogDebug("SQLite({@Key}, 1..{Version}) deleting: succeeded.", keyId, upToVersion);
        return true;
    }

    private async Task CleanupKeys(StorageDbContext context, CancellationToken token)
    {
        logger.LogDebug("SQLite(*) deleting: key cleanup begins.");

        var keys = await context.HistoricalKeys
            .Where(key => !context.HistoricalValues.Any(x => x.KeyId == key.Id && x.ValueType == key.ValueType))
            .Select(x => new HistoricalKeyRecord(x.Id, null!, null!, x.ValueType))
            .ToArrayAsync(token);

        logger.LogDebug("SQLite(*) deleting: keys found {Count}.", keys.Length);

        try
        {
            context.HistoricalKeys.AttachRange(keys);
            context.HistoricalKeys.RemoveRange(keys);
            await context.SaveChangesAsync(token);
        }
        catch (DbUpdateException ex)
        {
            logger.LogDebug(ex, "SQLite(*) deleting: already modified concurrently.");
            context.ChangeTracker.Clear();
        }

        logger.LogDebug("SQLite(*) deleting: key cleanup succeeded.");
    }
}
