using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using static Assistant.Net.Storage.SqliteNames;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     SQLite storage database context.
/// </summary>
public sealed class StorageDbContext : DbContext
{
    /// <summary/>
    public StorageDbContext(DbContextOptions<StorageDbContext> options) : base(options) { }

    /// <summary>
    ///     Storage keys.
    /// </summary>
    public DbSet<StorageKeyRecord> StorageKeys { get; set; } = null!;

    /// <summary>
    ///     Storage values.
    /// </summary>
    public DbSet<StorageValueRecord> StorageValues { get; set; } = null!;

    /// <summary>
    ///     Historical storage keys.
    /// </summary>
    public DbSet<HistoricalKeyRecord> HistoricalKeys { get; set; } = null!;

    /// <summary>
    ///     Historical storage values.
    /// </summary>
    public DbSet<HistoricalValueRecord> HistoricalValues { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureStorage(modelBuilder);
        ConfigureHistoricalStorage(modelBuilder);
    }

    private static void ConfigureStorage(ModelBuilder modelBuilder)
    {
        var keyBuilder = modelBuilder.Entity<StorageKeyRecord>();
        keyBuilder.ToTable(StorageKeysTableName)
            .HasKey(x => new {x.Id, x.ValueType});
        keyBuilder.Property(x => x.Id)
            .HasMaxLength(40);
        keyBuilder.Property(x => x.Type)
            .HasMaxLength(256);
        keyBuilder.Property(x => x.Content)
            .HasConversion(x => Convert.ToBase64String(x), x => Convert.FromBase64String(x));

        var valueBuilder = modelBuilder.Entity<StorageValueRecord>();
        valueBuilder.ToTable(StorageValuesTableName)
            .HasKey(x => new {x.KeyId, x.ValueType});
        valueBuilder.Property(x => x.KeyId)
            .HasMaxLength(40);
        valueBuilder.Property(x => x.ValueType)
            .HasMaxLength(256);
        valueBuilder.Property(x => x.ValueContent)
            .HasConversion(x => Convert.ToBase64String(x), x => Convert.FromBase64String(x));
        valueBuilder.Property(x => x.Version)
            .IsConcurrencyToken();
        valueBuilder.HasOne<StorageKeyRecord>()
            .WithMany()
            .HasForeignKey(x => new {x.KeyId, x.ValueType})
            .OnDelete(DeleteBehavior.Cascade);
        valueBuilder.OwnsMany(x => x.Details, b =>
        {
            var principalKey = b.Metadata.PrincipalKey.DeclaringEntityType.FindPrimaryKey()
                               ?? throw new ArgumentException("Primary key is required.", nameof(b));
            var principalKeyPropertyNames = principalKey.Properties.Select(x => x.Name).ToArray();
            b.WithOwner()
                .HasForeignKey(principalKeyPropertyNames);

            var keyPropertyNames = principalKeyPropertyNames.Append(nameof(StorageValueDetail.Name)).ToArray();
            b.ToTable(StorageDetailsTableName)
                .HasKey(keyPropertyNames);
        });
    }

    private static void ConfigureHistoricalStorage(ModelBuilder modelBuilder)
    {
        var keyBuilder = modelBuilder.Entity<HistoricalKeyRecord>();
        keyBuilder.ToTable(HistoricalKeysTableName)
            .HasKey(x => new {x.Id, x.ValueType});
        keyBuilder.Property(x => x.Id)
            .HasMaxLength(40);
        keyBuilder.Property(x => x.Type)
            .HasMaxLength(256);
        keyBuilder.Property(x => x.Content)
            .HasConversion(x => Convert.ToBase64String(x), x => Convert.FromBase64String(x));

        var valueBuilder = modelBuilder.Entity<HistoricalValueRecord>();
        valueBuilder.ToTable(HistoricalValuesTableName)
            .HasKey(x => new {x.KeyId, x.ValueType, x.Version});
        valueBuilder.Property(x => x.KeyId)
            .HasMaxLength(40);
        valueBuilder.Property(x => x.ValueType)
            .HasMaxLength(256);
        valueBuilder.Property(x => x.ValueContent)
            .HasConversion(x => Convert.ToBase64String(x), x => Convert.FromBase64String(x));
        valueBuilder.Property(x => x.Version)
            .IsConcurrencyToken();
        valueBuilder.HasOne<HistoricalKeyRecord>()
            .WithMany()
            .HasForeignKey(x => new {x.KeyId, x.ValueType})
            .OnDelete(DeleteBehavior.Cascade);
        valueBuilder.OwnsMany(x => x.Details, b =>
        {
            var principalKey = b.Metadata.PrincipalKey.DeclaringEntityType.FindPrimaryKey()
                               ?? throw new ArgumentException("Primary key is required.", nameof(b));
            var principalKeyPropertyNames = principalKey.Properties.Select(x => x.Name).ToArray();
            b.WithOwner()
                .HasForeignKey(principalKeyPropertyNames);

            var keyPropertyNames = principalKeyPropertyNames.Append(nameof(StorageValueDetail.Name)).ToArray();
            b.ToTable(HistoricalDetailsTableName)
                .HasKey(keyPropertyNames);
        });
    }
}
