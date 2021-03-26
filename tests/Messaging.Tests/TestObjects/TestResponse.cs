namespace Assistant.Net.Messaging.Tests
{
    public class TestResponse
    {
        public TestResponse(bool fail)
        {
            this.Fail = fail;
        }

        public bool Fail { get; }
    }
}