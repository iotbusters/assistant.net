using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Tests
{
    public class TestCommand1 : ICommand<TestResponse>
    {
        public TestCommand1(bool fail)
        {
            this.Fail = fail;
        }

        public bool Fail { get; }
    }

    public class TestCommand2 : ICommand
    {
        public TestCommand2(bool fail)
        {
            this.Fail = fail;
        }

        public bool Fail { get; }
    }
}