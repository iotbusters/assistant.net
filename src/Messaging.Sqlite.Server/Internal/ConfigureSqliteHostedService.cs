using Assistant.Net.Messaging.Options;
using Assistant.Net.Storage.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Messaging.Internal;

internal class ConfigureSqliteHostedService : IHostedService
{
    private readonly ILogger<ConfigureSqliteHostedService> logger;
    private readonly IServiceScopeFactory scopeFactory;

    public ConfigureSqliteHostedService(ILogger<ConfigureSqliteHostedService> logger, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken token)
    {
        await using var scope = scopeFactory.CreateAsyncScopeWithNamedOptionContext(GenericOptionsNames.DefaultName);
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StorageDbContext>>();
        var dbContext = await dbContextFactory.CreateDbContextAsync(token);

        try
        {
            await dbContext.Database.EnsureCreatedAsync(token);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to add create database.");
        }
    }

    public Task StopAsync(CancellationToken token) => Task.CompletedTask;
}
