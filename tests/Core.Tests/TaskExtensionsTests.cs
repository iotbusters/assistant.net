using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Assistant.Net.Core.Tests
{
    public class TaskExtensionsTests
    {
        [Test]
        public async Task When_calledAfter_succeeded()
        {
            var list = new ConcurrentBag<string>();
            var task = Task.Run(() =>
            {
                list.Add("start");
                return 0;
            });
            _ = task.When(x => list.Add("#1"), x => list.Add("#2"));

            await task;
            await Task.Yield();

            list.Should().BeEquivalentTo("start", "#1");
        }

        [Test]
        public async Task When_calledAfter_faulted()
        {
            var list = new ConcurrentBag<string>();
            var task = Task.Run(new Func<int>(() =>
            {
                list.Add("start");
                throw new Exception();
            }));
            _ = task.When(x => list.Add("#1"), x => list.Add("#2"));

            task.Awaiting(x => x).Should().ThrowExactly<Exception>();
            await Task.Yield();

            list.Should().BeEquivalentTo("start", "#2");
        }

        [Test]
        public async Task WhenSuccess_calledAfter_succeeded()
        {
            var list = new ConcurrentBag<string>();
            var task = Task.Run(() =>
            {
                list.Add("start");
                return 0;
            });
            _ = task.WhenSuccess(x => list.Add("#1"));
            _ = task.WhenSuccess(x => list.Add("#2"));

            await task;
            await Task.Yield();
            await Task.Yield();

            list.Should().BeEquivalentTo("start", "#1", "#2");
        }

        [Test]
        public async Task WhenSuccess_ignored_faulted()
        {
            var list = new ConcurrentBag<string>();
            var task = Task.Run(new Func<int>(() => throw new Exception()));
            _ = task.WhenSuccess(x => list.Add("#1"));

            task.Awaiting(x => x).Should().ThrowExactly<Exception>();
            await Task.Yield();

            list.Should().BeEmpty();
        }

        [Test]
        public async Task WhenFaulted_calledAfter_faulted()
        {
            var list = new ConcurrentBag<string>();
            var task = Task.Run(new Func<int>(() => throw new Exception()));
            _ = task.WhenFaulted(x => list.Add("#1"));
            _ = task.WhenFaulted(x => list.Add("#2"));

            task.Awaiting(x => x).Should().ThrowExactly<Exception>();
            await Task.Yield();
            await Task.Yield();

            list.Should().BeEquivalentTo("#1", "#2");
        }

        [Test]
        public async Task WhenFaulted_ignored_succeeded()
        {
            var list = new ConcurrentBag<string>();
            var task = Task.Run(() => 0);
            _ = task.WhenFaulted(x => list.Add("#1"));

            await task;
            await Task.Yield();

            list.Should().BeEmpty();
        }

        [Test]
        public async Task WhenSuccess_ignored_cancelled()
        {
            var list = new ConcurrentBag<string>();
            var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();
            var task = Task.FromCanceled<int>(cancellationSource.Token);
            _ = task.WhenSuccess(x => list.Add("#1"));

            task.Awaiting(x => x).Should().ThrowExactly<TaskCanceledException>();
            await Task.Yield();

            list.Should().BeEmpty();
        }

        [Test]
        public async Task WhenFaulted_ignored_cancelled()
        {
            var list = new ConcurrentBag<string>();
            var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();
            var task = Task.FromCanceled<int>(cancellationSource.Token);
            _ = task.WhenFaulted(x => list.Add("#1"));

            task.Awaiting(x => x).Should().ThrowExactly<TaskCanceledException>();
            await Task.Yield();

            list.Should().BeEmpty();
        }
    }
}