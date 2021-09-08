using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Core.Tests
{
    public class TaskExtensionsTests
    {
        [Test]
        public async Task Map_callsFaultSelector_taskFaulted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromException<int>(new Exception())
                .Map(
                    completeSelector: x =>
                    {
                        list.Add("completed");
                        return x;
                    },
                    faultSelector: x =>
                    {
                        list.Add("faulted");
                        return x;
                    })
                .Awaiting(_ => _).Should().ThrowExactlyAsync<Exception>();

            list.Should().BeEquivalentTo("faulted");
        }

        [Test]
        public async Task Map_callsCompleteSelector_taskCompleted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromResult(0)
                .Map(
                    completeSelector: x =>
                    {
                        list.Add("completed");
                        return x;
                    },
                    faultSelector: x =>
                    {
                        list.Add("faulted");
                        return x;
                    });

            list.Should().BeEquivalentTo("completed");
        }

        [Test]
        public async Task MapCompleted_callsSelector_taskCompleted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromResult(0)
                .MapCompleted(x =>
                {
                    list.Add("completed");
                    return x;
                });

            list.Should().BeEquivalentTo("completed");
        }

        [Test]
        public async Task MapCompleted_ignoresSelector_taskFaulted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromException<int>(new Exception())
                .MapCompleted(x =>
                {
                    list.Add("completed");
                    return x;
                })
                .Awaiting(_ => _).Should().ThrowExactlyAsync<Exception>();

            list.Should().BeEmpty();
        }

        [Test]
        public async Task MapFaulted_callsSelector_taskFaulted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromException<int>(new Exception())
                .MapFaulted(x =>
                {
                    list.Add("faulted");
                    return x;
                })
                .Awaiting(_ => _).Should().ThrowExactlyAsync<Exception>();

            list.Should().BeEquivalentTo("faulted");
        }

        [Test]
        public async Task MapFaulted_ignoresSelector_tasCompleted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromResult(0)
                .MapFaulted(x =>
                {
                    list.Add("faulted");
                    return x;
                });

            list.Should().BeEmpty();
        }

        [Test]
        public async Task MapOfTaskOfResult_callsFaultSelector_taskFaulted()
        {
            var list = new ConcurrentBag<string>();

            await Task.Run(() => Task.FromException<int>(new Exception()))
                .Map(
                    completeSelector: x =>
                    {
                        list.Add("completed");
                        return x;
                    },
                    faultSelector: x =>
                    {
                        list.Add("faulted");
                        return x;
                    })
                .Awaiting(_ => _).Should().ThrowExactlyAsync<Exception>();

            list.Should().BeEquivalentTo("faulted");
        }

        [Test]
        public async Task MapOfTaskOfResult_callsCompleteSelector_taskCompleted()
        {
            var list = new ConcurrentBag<string>();

            await Task.Run(() => Task.FromResult(0))
                .Map(
                    completeSelector: x =>
                    {
                        list.Add("completed");
                        return x;
                    },
                    faultSelector: x =>
                    {
                        list.Add("faulted");
                        return x;
                    });

            list.Should().BeEquivalentTo("completed");
        }

        [Test]
        public async Task MapCompletedOfTaskOfResult_callsSelector_taskCompleted()
        {
            var list = new ConcurrentBag<string>();

            await Task.Run(() => Task.FromResult(0))
                .MapCompleted(x =>
                {
                    list.Add("completed");
                    return x;
                });

            list.Should().BeEquivalentTo("completed");
        }

        [Test]
        public async Task MapCompletedOfTaskOfResult_ignoresSelector_taskFaulted()
        {
            var list = new ConcurrentBag<string>();

            await Task.Run(() => Task.FromException<int>(new Exception()))
                .MapCompleted(x =>
                {
                    list.Add("completed");
                    return x;
                })
                .Awaiting(_ => _).Should().ThrowExactlyAsync<Exception>();

            list.Should().BeEmpty();
        }

        [Test]
        public async Task When_callsCompleteSelector_taskCompleted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromResult(0)
                .When(
                    completeAction: _ => list.Add("completed"),
                    faultAction: _ => list.Add("faulted"));

            list.Should().BeEquivalentTo("completed");
        }

        [Test]
        public async Task When_callsFaultAction_taskFaulted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromException<int>(new Exception())
                .When(
                    completeAction: _ => list.Add("completed"),
                    faultAction: _ => list.Add("faulted"))
                .Awaiting(_ => _).Should().ThrowExactlyAsync<Exception>();

            list.Should().BeEquivalentTo("faulted");
        }

        [Test]
        public async Task WhenComplete_callsSelector_taskCompleted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromResult(0)
                .WhenComplete(_ => list.Add("completed"));

            list.Should().BeEquivalentTo("completed");
        }

        [Test]
        public async Task WhenComplete_ignoresSelector_taskFaulted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromException<int>(new Exception())
                .WhenComplete(_ => list.Add("completed"))
                .Awaiting(_ => _).Should().ThrowExactlyAsync<Exception>();

            list.Should().BeEmpty();
        }

        [Test]
        public async Task WhenFaulted_callsSelector_taskFaulted()
        {
            var list = new ConcurrentBag<string>();

            await Task.FromException<int>(new Exception())
                .WhenFaulted(_ => list.Add("faulted"))
                .Awaiting(_ => _).Should().ThrowExactlyAsync<Exception>();

            list.Should().BeEquivalentTo("faulted");
        }

        [Test]
        public async Task WhenFaulted_ignoresSelector_taskCompleted()
        {
            var list = new ConcurrentBag<string>();

            await Task.Run(() => 0)
                .WhenFaulted(_ => list.Add("completed"));

            list.Should().BeEmpty();
        }

        [Test]
        public async Task WhenComplete_ignoresSelector_taskCancelled()
        {
            var list = new ConcurrentBag<string>();
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            await Task.FromCanceled<int>(cancellationSource.Token)
                .WhenComplete(_ => list.Add("completed"))
                .Awaiting(_ => _).Should().ThrowExactlyAsync<TaskCanceledException>();

            list.Should().BeEmpty();
        }

        [Test]
        public async Task WhenFaulted_ignoresSelector_taskCancelled()
        {
            var list = new ConcurrentBag<string>();
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            await Task.FromCanceled<int>(cancellationSource.Token)
                .WhenFaulted(_ => list.Add("completed"))
                .Awaiting(_ => _).Should().ThrowExactlyAsync<TaskCanceledException>();

            list.Should().BeEmpty();
        }
    }
}