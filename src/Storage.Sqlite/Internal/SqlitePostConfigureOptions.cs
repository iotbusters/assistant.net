using Assistant.Net.Options;
using Assistant.Net.Storage.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Assistant.Net.Storage.Internal;

internal class SqlitePostConfigureOptions : IPostConfigureOptions<SqliteOptions>
{
    private readonly ILogger<SqlitePostConfigureOptions> logger;
    private readonly HashSet<string> created = new();

    public SqlitePostConfigureOptions(ILogger<SqlitePostConfigureOptions> logger) =>
        this.logger = logger;

    public void PostConfigure(string name, SqliteOptions options)
    {
        if(created.Contains(name) || !options.EnsureDatabaseCreated)
            return;

        logger.LogInformation("Ensure database is created.");

        var dbContextOptions = new DbContextOptionsBuilder<StorageDbContext>().UseSqlite(options).Options;
        using var dbContext = new StorageDbContext(dbContextOptions);

        try
        {
            dbContext.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to add create database.");
            return;
        }

        logger.LogInformation("Database is created.");

        created.Add(name);
    }
}
