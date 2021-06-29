using NUnit.Framework;

namespace Assistant.Net.Serialization.Json.Tests.Mocks
{
    public class TestObjectUnserializable
    {
        public TestObjectUnserializable(string unknown)
        {
            Assert.Fail("Invalid ctor was selected.");
        }
    }
}