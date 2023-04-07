using Assistant.Net.Core.Tests.Utils;
using Assistant.Net.Utils;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Assistant.Net.Core.Tests.Performance;

public class HashExtensionsPerformanceTests
{
    [TestCaseSource(typeof(HashExtensionsTests),nameof(HashExtensionsTests.GetSha1Values))]
    public void GetSha1_generatesInTime(object value)
    {
        var count = 1000;
        value.GetSha1();// to avoid first run impact

        var watch2 = Stopwatch.StartNew();
        for (var i = 0; i < count; i++) value.GetSha1();
        watch2.Stop();
        Console.WriteLine($"Total: {watch2.Elapsed} (0.006) Middle: {watch2.Elapsed / count}");
        watch2.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(0.04));
    }

    [TestCaseSource(typeof(HashExtensionsTests), nameof(HashExtensionsTests.GetSha256Values))]
    public void GetSha256_generatesInTime(object value)
    {
        var count = 1000;
        value.GetSha256();// to avoid first run impact

        var watch2 = Stopwatch.StartNew();
        for (var i = 0; i < count; i++) value.GetSha256();
        watch2.Stop();
        Console.WriteLine($"Total: {watch2.Elapsed} (0.006) Middle: {watch2.Elapsed / count}");
        watch2.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(0.04));
    }

    [TestCaseSource(typeof(HashExtensionsTests), nameof(HashExtensionsTests.GetMd5Values))]
    public void GetMd5_generatesInTime(object value)
    {
        var count = 1000;
        value.GetMd5();// to avoid first run impact

        var watch2 = Stopwatch.StartNew();
        for (var i = 0; i < count; i++) value.GetMd5();
        watch2.Stop();
        Console.WriteLine($"Total: {watch2.Elapsed} (0.006) Middle: {watch2.Elapsed / count}");
        watch2.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(0.04));
    }
}
