using Assistant.Net.Abstractions;
using Assistant.Net.Options;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests;

[SetUpFixture]
public class SetupMongo
{
    private ServiceProvider? provider;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        provider = new ServiceCollection()
            .AddMongoClient()
            .ConfigureMongoOptions(ConfigureMongo)
            .BuildServiceProvider();

        var client = provider.GetRequiredService<IMongoClient>();

        string pingContent;
        try
        {

            var ping = await client.GetDatabase("db").RunCommandAsync(
                (Command<BsonDocument>)"{ping:1}",
                ReadPreference.Nearest,
                CancellationToken);
            pingContent = ping.ToString();
        }
        catch
        {
            pingContent = string.Empty;
        }

        if (!pingContent.Contains("ok"))
            Assert.Ignore($"The tests require mongodb instance at {ConnectionString}.");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (provider == null)
            return;

        var options = provider.GetRequiredService<INamedOptions<MongoOptions>>().Value;
        var client = provider.GetRequiredService<IMongoClient>();
        await client.DropDatabaseAsync(options.DatabaseName, CancellationToken);

        await provider.DisposeAsync();
    }

    public static void ConfigureMongo(MongoOptions options) => options.Connection(ConnectionString).Database("test").EnsureCreated();

    private const string ConnectionString = "mongodb://127.0.0.1:27017";

    private static CancellationToken CancellationToken => new CancellationTokenSource(1000).Token;
}
