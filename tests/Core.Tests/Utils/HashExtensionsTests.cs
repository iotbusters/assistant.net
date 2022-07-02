using Assistant.Net.Utils;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace Assistant.Net.Core.Tests.Utils;

public class HashExtensionsTests
{
    [TestCaseSource("GetValues")]
    public void GetSha1_returnsHashCode(object value, string hashCode) =>
        value.GetSha1().Should().Be(hashCode);

    public static IEnumerable<TestCaseData> GetValues()
    {
        yield return new TestCaseData("123", "Pb03ubPumh30hI2jvQc7Sb96DIY=");
        yield return new TestCaseData(123, "GVmJP2giBFnL2AA5bh6ue/w4Lpc=");
        yield return new TestCaseData(123f, "wZfwyOyfHytX8daUfciYnPtBjPs=");
        yield return new TestCaseData(123d, "ngna5ErtpwZPDQJlwV2O+kauna4=");
        yield return new TestCaseData(true, "v4tFMNjSRt10rFOhNHG7oXlB3/c=");
        yield return new TestCaseData((byte)1, "v4tFMNjSRt10rFOhNHG7oXlB3/c=");
        yield return new TestCaseData(Encoding.UTF8.GetBytes("123"), "QL0AFWMIX8NRZTKeof9cXsvbvu8=");
        yield return new TestCaseData(new[] {1, 2, 3}, "5CnMo/cDo5zFlUplcv7JCGE1s04=");
        yield return new TestCaseData(new {String = "value", Int = "int", Object = new {Array = new[] {1.2, 2.3}}}, "z3Y4vBdo68DeeDEbZRTytCTkljU=");
    }
}
