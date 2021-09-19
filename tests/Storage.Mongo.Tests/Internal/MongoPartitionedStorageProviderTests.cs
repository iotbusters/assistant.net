using Assistant.Net.Storage.Abstractions;
using Assistant.Net.Storage.Mongo.Models;
using Assistant.Net.Storage.Mongo.Tests.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Mongo.Tests.Internal
{
    public class MongoPartitionedStorageProviderTests
    {
    }
}
