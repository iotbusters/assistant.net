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
            logger.LogInformation("Storage.AddOrGet({KeyId}): {Attempt} begins.", key.Id, attempt);

            if (await TryGet(key, token) is Some<ValueRecord>(var found))
            {
                logger.LogInformation("Storage.AddOrGet({KeyId}, {Version}): {Attempt} found.",
                    key.Id, found.Audit.Version, attempt);
                return found;
            }

            if (await InsertValue(key, addFactory, token) is Some<ValueRecord>(var added))
            {
                logger.LogInformation("Storage.AddOrGet({KeyId}, {Version}): {Attempt} added.", key.Id, 1, attempt);
                return added;
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrGet({KeyId}, {Version}): {Attempt} reached the limit.", key.Id, 1, attempt);
                throw new StorageConcurrencyException();
            }

            logger.LogWarning("Storage.AddOrGet({KeyId}, {Version}): {Attempt} failed.", key.Id, 1, attempt);
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
        var storageDbContext = CreateDbContext();

        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrUpdate({KeyId}): {Attempt} begins.", key.Id, attempt);

            if (await storageDbContext.StorageValues.AnyAsync(x => x.KeyId == key.Id && x.ValueType == key.ValueType, token))
            {
                if (await UpdateValue(key, updateFactory, token) is Some<ValueRecord>(var updated))
                {
                    logger.LogInformation("Storage.AddOrUpdate({KeyId}, {Version}): {Attempt} updated.",
                        key.Id, updated.Audit.Version, attempt);
                    return updated;
                }
            }
            else
            {
                if (await InsertValue(key, addFactory, token) is Some<ValueRecord>(var added))
                {
                    logger.LogInformation("Storage.AddOrUpdate({KeyId}, {Version}): {Attempt} added.",
                        key.Id, added.Audit.Version, attempt);
                    return added;
                }
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrUpdate({KeyId}): {Attempt} reached the limit.", key.Id, attempt);
                throw new StorageConcurrencyException();
            }

            logger.LogWarning("Storage.AddOrUpdate({KeyId}): {Attempt} failed.", key.Id, attempt);
            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token) =>
        await FindRecord(CreateDbContext().StorageValues.AsNoTracking(), key, token).MapOption(ToValue);

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token) =>
        await DeleteValue(key, token);

    public IQueryable<KeyRecord> GetKeys() =>
        CreateDbContext().StorageKeys.AsNoTracking().Select(x => new KeyRecord
        {
            Id = x.Id,
            Type = x.Type,
            Content = x.Content,
            ValueType = x.ValueType
        });

    public void Dispose() { }

    private StorageDbContext CreateDbContext() =>
        dbContextFactory.CreateDbContext();

    private ValueRecord ToValue(StorageValueRecord record) =>
        new(record.ValueType, record.ValueContent, new Audit(record.Details.FromDetailArray(), record.Version));

    private async Task<Option<StorageValueRecord>> FindRecord(IQueryable<StorageValueRecord> values, KeyRecord key, CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}) querying: begins.", key.Id);

        var found = await values.SingleOrDefaultAsync(x => x.KeyId == key.Id && x.ValueType == key.ValueType, token);
        if (found != null)
            logger.LogDebug("SQLite({KeyId})[{Version}] querying: found.", key.Id, found.Version);
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

        if (await dbContext.StorageKeys.AnyAsync(x => x.Id == key.Id && x.ValueType == key.ValueType, token))
            logger.LogDebug("SQLite({KeyId}, {Version}) key: found.", key.Id, added.Audit.Version);
        else
        {
            var keyRecord = new StorageKeyRecord(key.Id, key.Type, key.Content, key.ValueType);
            await dbContext.AddAsync(keyRecord, token);

            logger.LogDebug("SQLite({KeyId}, {Version}) key: adding.", key.Id, added.Audit.Version);
        }

        try
        {
            await dbContext.AddAsync(valueRecord, token);
            await dbContext.SaveChangesAsync(token);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 1555})
        {
            // note: primary key constraint violation (https://www.sqlite.org/rescode.html#constraint_primarykey)
            logger.LogWarning(ex, "SQLite({KeyId}, {Version}) inserting: already present.", key.Id, added.Audit.Version);
            return Option.None;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 787})
        {
            // note: foreign key constraint violation (https://www.sqlite.org/rescode.html#constraint_foreignkey)
            logger.LogWarning(ex, "SQLite({KeyId}, {Version}) inserting: key concurrently deleted.", key.Id, added.Audit.Version);
            return Option.None;
        }

        logger.LogDebug("SQLite({KeyId}, {Version}) inserting: succeeded.", key.Id, added.Audit.Version);
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

        logger.LogDebug("SQLite({KeyId})[{Version}] updating: begins.",
            key.Id, newValue.Audit.Version);

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
            logger.LogWarning(ex, "SQLite({KeyId})[{Version}] updating: already modified concurrently.",
                key.Id, newValue.Audit.Version);
            return Option.None;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 787})
        {
            // note: foreign key constraint violation (https://www.sqlite.org/rescode.html#constraint_foreignkey)
            logger.LogWarning(ex, "SQLite({KeyId})[{Version}] inserting: key concurrently deleted.",
                key.Id, newValue.Audit.Version);
            return Option.None;
        }

        logger.LogDebug("SQLite({KeyId})[{Version}] updating: succeeded.",
            key.Id, newValue.Audit.Version);
        return Option.Some(newValue);
    }

    private async Task<Option<ValueRecord>> DeleteValue(KeyRecord key, CancellationToken token)
    {
        logger.LogDebug("SQLite({KeyId}) deleting: begins.", key.Id);

        var dbContext = CreateDbContext();
        if (await FindRecord(dbContext.StorageValues, key, token) is not Some<StorageValueRecord>(var found))
        {
            logger.LogDebug("SQLite({KeyId}) deleting: not found.", key.Id);
            return Option.None;
        }

        try
        {
            dbContext.Remove(found);
            await dbContext.SaveChangesAsync(token);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "SQLite({KeyId})[{Version}] deleting: already deleted.", key.Id, found.Version);
            return Option.None;
        }

        logger.LogDebug("SQLite({KeyId})[{Version}] deleting: succeeded.", key.Id, found.Version);
        return ToValue(found).AsOption();
    }
}
