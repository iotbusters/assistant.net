﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using static Assistant.Net.Storage.SqliteNames;

namespace Assistant.Net.Storage.Models;

/// <summary>
///     SQLite historical storage database context.
/// </summary>
public class HistoricalStorageDbContext : DbContext
{
    /// <summary/>
    public HistoricalStorageDbContext(DbContextOptions<HistoricalStorageDbContext> options) : base(options) { }

    /// <summary>
    ///     Storage keys.
    /// </summary>
    public DbSet<SqliteKeyRecord> Keys { get; set; } = null!;

    /// <summary>
    ///     Historical storage values.
    /// </summary>
    public DbSet<SqliteRecord> Values { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var keyBuilder = modelBuilder.Entity<SqliteKeyRecord>();
        keyBuilder.ToTable(KeysTableName, HistoricalStorageSchemaName)
            .HasKey(x => x.Id);
        keyBuilder.Property(x => x.Id)
            .HasMaxLength(40);
        keyBuilder.Property(x => x.Type)
            .HasMaxLength(256);
        keyBuilder.Property(x => x.Content)
            .HasConversion(x => Convert.ToBase64String(x), x => Convert.FromBase64String(x));

        var valueBuilder = modelBuilder.Entity<SqliteRecord>();
        valueBuilder.ToTable(ValuesTableName, HistoricalStorageSchemaName)
            .HasKey(x => new {x.KeyId, x.Version});
        valueBuilder.Property(x => x.KeyId)
            .HasMaxLength(40);
        valueBuilder.Property(x => x.ValueType)
            .HasMaxLength(256);
        valueBuilder.Property(x => x.ValueContent)
            .HasConversion(x => Convert.ToBase64String(x), x => Convert.FromBase64String(x));
        valueBuilder.Property(x => x.Version)
            .IsConcurrencyToken();
        valueBuilder.HasOne<SqliteKeyRecord>()
            .WithMany()
            .HasForeignKey(x => x.KeyId)
            .OnDelete(DeleteBehavior.Cascade);
        valueBuilder.OwnsMany(x => x.Details, b =>
        {
            var principalKey = b.Metadata.PrincipalKey.DeclaringEntityType.FindPrimaryKey()
                               ?? throw new ArgumentException("Primary key is required.", nameof(b));
            var principalKeyPropertyNames = principalKey.Properties.Select(x => x.Name).ToArray();
            b.WithOwner()
                .HasForeignKey(principalKeyPropertyNames);

            var keyPropertyNames = principalKeyPropertyNames.Append(nameof(Detail.Name)).ToArray();
            b.ToTable(DetailsTableName, HistoricalStorageSchemaName)
                .HasKey(keyPropertyNames);
        });
    }
}