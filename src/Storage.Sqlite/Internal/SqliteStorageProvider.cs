using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Exceptions;
using Assistant.Net.Storage.Models;
using Assistant.Net.Storage.Options;
using Assistant.Net.Unions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Internal;

internal class SqliteStorageProvider<TValue> : IStorageProvider<TValue>
{
    private readonly ILogger logger;
    private readonly SqliteStoringOptions options;
    private readonly IDbContextFactory<StorageDbContext> dbContextFactory;

    public SqliteStorageProvider(
        ILogger<SqliteStorageProvider<TValue>> logger,
        IOptions<SqliteStoringOptions> options,
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
                return found;

            if (await InsertValue(key, addFactory, token) is Some<ValueRecord>(var added))
                return added;

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

            var storageDbContext = CreateDbContext();
            if (await storageDbContext.StorageValues.AnyAsync(x => x.KeyId == key.Id, token))
            {
                if (await UpdateValue(key, updateFactory, token) is Some<ValueRecord>(var updated))
                {
                    logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} succeeded.", key.Id, attempt);
                    return updated;
                }
            }
            else
            {
                if (await InsertValue(key, addFactory, token) is Some<ValueRecord>(var added))
                {
                    logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} succeeded.", key.Id, attempt);
                    return added;
                }
            }

            logger.LogDebug("Storage.AddOrUpdate({KeyId}): {Attempt} failed.", key.Id, attempt);

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

    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token) =>
        await FindRecord(CreateDbContext().StorageValues.AsNoTracking(), key, token).MapOption(ToValue);

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token) =>
        await DeleteValue(key, token);

    public IQueryable<KeyRecord> GetKeys() =>
        CreateDbContext().StorageKeys.AsNoTracking().Select(x => new KeyRecord(x.Id, x.Type, x.Content));

    public void Dispose() { }

    private StorageDbContext CreateDbContext() =>
        dbContextFactory.CreateDbContext();

    private ValueRecord ToValue(StorageValueRecord record) =>
        new(record.ValueType, record.ValueContent, new Audit(record.Details.FromDetailArray(), record.Version));

    private async Task<Option<StorageValueRecord>> FindRecord(IQueryable<StorageValueRecord> values, KeyRecord key, CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}) querying: begins.", key.Id);

        var found = await values.SingleOrDefaultAsync(x => x.KeyId == key.Id, token);
        if (found != null)
            logger.LogDebug("SQLite({KeyId}:{Version}) querying: found.", key.Id, found.Version);
        else
            logger.LogDebug("SQLite({KeyId}) querying: not found.", key.Id);

        return found.AsOption();
    }

    private async Task<Option<ValueRecord>> InsertValue(
        KeyRecord key,
        Func<KeyRecord, Task<ValueRecord>> addFactory,
        CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}) inserting: begins.", key.Id);

        var added = await addFactory(key);
        var valueRecord = new StorageValueRecord(
            key.Id,
            added.Type,
            added.Content,
            added.Audit.Version,
            added.Audit.Details.ToDetailArray());

        var dbContext = CreateDbContext();

        if (await dbContext.StorageKeys.AnyAsync(x => x.Id == key.Id, token))
            logger.LogDebug("SQLite({KeyId}) key: found.", key.Id);
        else
        {
            var keyRecord = new StorageKeyRecord(key.Id, key.Type, key.Content);
            await dbContext.AddAsync(keyRecord, token);

            logger.LogDebug("SQLite({KeyId}) key: adding.", key.Id);
        }

        try
        {
            await dbContext.AddAsync(valueRecord, token);
            await dbContext.SaveChangesAsync(token);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 1555})
        {
            // note: primary key constraint violation (https://www.sqlite.org/rescode.html#constraint_primarykey)
            logger.LogWarning(ex, "SQLite({KeyId}) inserting: already present.", key.Id);
            return Option.None;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 787})
        {
            // note: foreign key constraint violation (https://www.sqlite.org/rescode.html#constraint_foreignkey)
            logger.LogWarning(ex, "SQLite({KeyId}) inserting: key concurrently deleted.", key.Id);
            return Option.None;
        }

        logger.LogDebug("SQLite({KeyId}) inserting: succeeded.", key.Id);
        return Option.Some(added);
    }

    private async Task<Option<ValueRecord>> UpdateValue(
        KeyRecord key,
        Func<KeyRecord, ValueRecord, Task<ValueRecord>> updateFactory,
        CancellationToken token)
    {
        var dbContext = CreateDbContext();
        if (await FindRecord(dbContext.StorageValues, key, token) is not Some<StorageValueRecord>(var found))
            return Option.None;

        var oldValue = ToValue(found);
        var newValue = await updateFactory(key, oldValue);

        logger.LogDebug("SQLite({KeyId}:{OldVersion}/{NewVersion}) updating: begins.",
            key.Id, oldValue.Audit.Version, newValue.Audit.Version);

        found.Version = newValue.Audit.Version;
        found.ValueContent = newValue.Content;
        found.Details = newValue.Audit.Details.ToDetailArray();

        try
        {
            dbContext.Update(found);
            await dbContext.SaveChangesAsync(token);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "SQLite({KeyId}:{OldVersion}/{NewVersion}) updating: already modified concurrently.",
                key.Id, oldValue.Audit.Version, newValue.Audit.Version);
            return Option.None;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 787})
        {
            // note: foreign key constraint violation (https://www.sqlite.org/rescode.html#constraint_foreignkey)
            logger.LogWarning(ex, "SQLite({KeyId}:{OldVersion}/{NewVersion}) inserting: key concurrently deleted.",
                key.Id, oldValue.Audit.Version, newValue.Audit.Version);
            return Option.None;
        }

        logger.LogDebug("SQLite({KeyId}:{OldVersion}/{NewVersion}) updating: succeeded.",
            key.Id, oldValue.Audit.Version, newValue.Audit.Version);
        return Option.Some(newValue);
    }

    private async Task<Option<ValueRecord>> DeleteValue(KeyRecord key, CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}) deleting: begins.", key.Id);

        var dbContext = CreateDbContext();
        if (await FindRecord(dbContext.StorageValues, key, token) is not Some<StorageValueRecord>(var found))
            return Option.None;

        try
        {
            dbContext.Remove(found);
            await dbContext.SaveChangesAsync(token);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "SQLite({KeyId}) deleting: already deleted.", key.Id);
            return Option.None;
        }

        logger.LogDebug("SQLite({KeyId}) deleting: succeeded.", key.Id);
        return ToValue(found).AsOption();
    }
}
