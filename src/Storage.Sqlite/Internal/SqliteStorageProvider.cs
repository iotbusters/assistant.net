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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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
        var context = CreateDbContext();

        var keyId = new Key(key.Id, key.ValueType);
        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.AddOrGet({@Key}): {Attempt} begins.", keyId, attempt);

            var records = context.StorageValues.AsNoTracking();
            if (await FindValue(records, key, token) is Some<StorageValueRecord>(var found))
            {
                logger.LogInformation("Storage.AddOrGet({@Key}, {Version}): {Attempt} found.", keyId, found.Version, attempt);
                return ToValue(found);
            }

            var value = await addFactory(key);
            var record = new StorageValueRecord(
                key.Id,
                key.ValueType,
                value.Content,
                value.Audit.Version,
                value.Audit.Details.ToDetailArray());
            if (await InsertValue(context, key, record, token))
            {
                logger.LogInformation("Storage.AddOrGet({@Key}, {Version}): {Attempt} added.", keyId, 1, attempt);
                return value;
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrGet({@Key}, {Version}): {Attempt} reached the limit.", keyId, 1, attempt);
                throw new StorageConcurrencyException();
            }

            logger.LogWarning("Storage.AddOrGet({@Key}, {Version}): {Attempt} failed.", keyId, 1, attempt);
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
            if (await FindValue(context.StorageValues, key, token) is Some<StorageValueRecord>(var found))
            {
                var updated = await updateFactory(key, ToValue(found));
                found.Version = updated.Audit.Version;
                found.ValueContent = updated.Content;
                found.Details = updated.Audit.Details.ToDetailArray();
                if (await UpdateValue(context, found, token))
                {
                    logger.LogInformation("Storage.AddOrUpdate({@Key}, {Version}): {Attempt} updated.", keyId, found.Version, attempt);
                    return updated;
                }
            }

            var value = await addFactory(key);
            var record = new StorageValueRecord(
                key.Id,
                key.ValueType,
                value.Content,
                value.Audit.Version,
                value.Audit.Details.ToDetailArray());
            if (await InsertValue(context, key, record, token))
            {
                logger.LogInformation("Storage.AddOrUpdate({@Key}, {Version}): {Attempt} added.", keyId, record.Version, attempt);
                return value;
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.AddOrUpdate({@Key}): {Attempt} reached the limit.", keyId, attempt);
                throw new StorageConcurrencyException();
            }

            logger.LogWarning("Storage.AddOrUpdate({@Key}): {Attempt} failed.", keyId, attempt);
            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async Task<Option<ValueRecord>> TryGet(KeyRecord key, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogInformation("Storage.TryGet({@Key}): begins.", keyId);

        var records = CreateDbContext().StorageValues.AsNoTracking();
        if (await FindValue(records, key, token) is Some<StorageValueRecord>(var found))
        {
            logger.LogInformation("Storage.TryGet({@Key}, {Version}): found.", keyId, found.Version);
            return ToValue(found).AsOption();
        }

        logger.LogInformation("Storage.TryGet({@Key}): not found.", keyId);
        return Option.None;
    }

    public async Task<Option<ValueRecord>> TryRemove(KeyRecord key, CancellationToken token)
    {
        var strategy = options.DeleteRetry;
        var context = CreateDbContext();

        var keyId = new Key(key.Id, key.ValueType);
        var attempt = 1;
        while (true)
        {
            logger.LogInformation("Storage.TryRemove({@Key}): {Attempt} begins.", keyId, attempt);

            if (await FindValue(context.StorageValues, key, token) is not Some<StorageValueRecord>(var found))
            {
                logger.LogInformation("Storage.TryRemove({@Key}): {Attempt} not found.", keyId, attempt);
                return Option.None;
            }

            if (await DeleteValue(context, found, token))
            {
                logger.LogInformation("Storage.TryRemove({@Key}, {Version}): {Attempt} deleted.", keyId, found.Version, attempt);
                return ToValue(found).AsOption();
            }

            attempt++;
            if (!strategy.CanRetry(attempt))
            {
                logger.LogError("Storage.TryRemove({@Key}): {Attempt} reached the limit.", keyId, attempt);
                throw new StorageConcurrencyException();
            }

            logger.LogWarning("Storage.TryRemove({@Key}): {Attempt} failed.", keyId, attempt);
            await Task.Delay(strategy.DelayTime(attempt), token);
        }
    }

    public async IAsyncEnumerable<KeyRecord> GetKeys(Expression<Func<KeyRecord, bool>> predicate, [EnumeratorCancellation] CancellationToken token)
    {
        logger.LogInformation("Storage.GetKeys(): begins.");

        var keys = CreateDbContext().StorageKeys.AsNoTracking()
            .Select(x => new KeyRecord {Id = x.Id, Type = x.Type, Content = x.Content, ValueType = x.ValueType})
            .Where(predicate)
            .AsAsyncEnumerable()
            .WithCancellation(token);

        await foreach (var key in keys)
            yield return key;

        logger.LogInformation("Storage.GetKeys(): succeeded.");
    }

    private StorageDbContext CreateDbContext() => dbContextFactory.CreateDbContext();

    private ValueRecord ToValue(StorageValueRecord record) =>
        new(record.ValueContent, new Audit(record.Details.FromDetailArray(), record.Version));

    private async Task<Option<StorageValueRecord>> FindValue(IQueryable<StorageValueRecord> values, KeyRecord key, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogDebug("SQLite({@Key}) querying: begins.", keyId);

        var found = await values.SingleOrDefaultAsync(x => x.KeyId == key.Id && x.ValueType == key.ValueType, token);
        if (found != null)
            logger.LogDebug("SQLite({@Key}, {Version}) querying: found.", keyId, found.Version);
        else
            logger.LogDebug("SQLite({@Key}) querying: not found.", keyId);

        return found.AsOption();
    }

    private async Task<bool> InsertValue(StorageDbContext context, KeyRecord key, StorageValueRecord record, CancellationToken token)
    {
        var keyId = new Key(key.Id, key.ValueType);

        logger.LogDebug("SQLite({@Key}) inserting: begins.", keyId);
        
        if (await context.StorageKeys.AnyAsync(x => x.Id == key.Id && x.ValueType == key.ValueType, token))
            logger.LogDebug("SQLite({@Key}, {Version}) inserting: key found.", keyId, record.Version);
        else
        {
            var keyRecord = new StorageKeyRecord(key.Id, key.Type, key.Content, key.ValueType);
            await context.AddAsync(keyRecord, token);

            logger.LogDebug("SQLite({@Key}, {Version}) inserting: key adding.", keyId, record.Version);
        }

        try
        {
            await context.AddAsync(record, token);
            await context.SaveChangesAsync(token);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 1555})
        {
            // note: primary key constraint violation (https://www.sqlite.org/rescode.html#constraint_primarykey)
            logger.LogWarning(ex, "SQLite({@Key}, {Version}) inserting: already present.", keyId, record.Version);
            context.ChangeTracker.Clear();
            return false;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 787})
        {
            // note: foreign key constraint violation (https://www.sqlite.org/rescode.html#constraint_foreignkey)
            logger.LogWarning(ex, "SQLite({@Key}, {Version}) inserting: key concurrently deleted.", keyId, record.Version);
            context.ChangeTracker.Clear();
            return false;
        }

        logger.LogDebug("SQLite({@Key}, {Version}) inserting: succeeded.", keyId, record.Version);
        return true;
    }

    private async Task<bool> UpdateValue(StorageDbContext context, StorageValueRecord found, CancellationToken token)
    {
        var keyId = new Key(found.KeyId, found.ValueType);
        
        logger.LogDebug("SQLite({@Key}, {Version}) updating: begins.", keyId, found.Version);
        
        try
        {
            context.Update(found);
            await context.SaveChangesAsync(token);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "SQLite({@Key}, {Version}) updating: already modified concurrently.", keyId, found.Version);
            context.ChangeTracker.Clear();
            return false;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException {SqliteExtendedErrorCode: 787})
        {
            // note: foreign key constraint violation (https://www.sqlite.org/rescode.html#constraint_foreignkey)
            logger.LogWarning(ex, "SQLite({@Key}, {Version}) inserting: key concurrently deleted.", keyId, found.Version);
            context.ChangeTracker.Clear();
            return false;
        }

        logger.LogDebug("SQLite({@Key}, {Version}) updating: succeeded.", keyId, found.Version);
        return true;
    }

    private async Task<bool> DeleteValue(StorageDbContext context, StorageValueRecord record, CancellationToken token)
    {
        var keyId = new Key(record.KeyId, record.ValueType);

        logger.LogDebug("SQLite({@Key}) deleting: begins.", keyId);

        try
        {
            var key = new StorageKeyRecord(record.KeyId, null!, null!, record.ValueType);
            context.StorageKeys.Attach(key);
            context.StorageKeys.Remove(key);
            context.StorageValues.Remove(record);
            await context.SaveChangesAsync(token);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "SQLite({@Key}, {Version}) deleting: concurrently deleted.", keyId, record.Version);
            context.ChangeTracker.Clear();
            return false;
        }

        logger.LogDebug("SQLite({@Key}, {Version}) deleting: succeeded.", keyId, record.Version);
        return true;
    }
}
