using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Messaging.Caching;
using Assistant.Net.Messaging.Exceptions;

namespace Assistant.Net.Messaging.Web.Server.Tests
{
    public class DeferredResultTests
    {
        [Test]
        public void Test1()
        {
            var task =  Task.Factory.StartNew(() => 10);
            var result = new DeferredResult(task);
            result.Invoking(x => x.Get())
                .Should().Throw<CommandDeferredException>();
        }

        [Test]
        public async Task Test2()
        {
            var task = Task.Factory.StartNew(() => 10);
            var result = new DeferredResult(task);
            await Task.Delay(100);
            result.Get().Should().Be(10);
        }
    }
}