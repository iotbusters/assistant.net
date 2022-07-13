using Assistant.Net.Core.Tests.Utils;
using Assistant.Net.Utils;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Assistant.Net.Core.Tests.Performance;

public class HashExtensionsPerformanceTests
{
    [TestCaseSource(typeof(HashExtensionsTests),nameof(HashExtensionsTests.GetValues))]
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

}
