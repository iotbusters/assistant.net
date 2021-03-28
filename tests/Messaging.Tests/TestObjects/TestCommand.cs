using System;
using Assistant.Net.Messaging.Abstractions;

namespace Assistant.Net.Messaging.Tests
{
    public class TestCommand1 : ICommand<TestResponse>
    {
        public TestCommand1(Exception? exception) => Exception = exception;

        public Exception? Exception { get; }
    }

    public class TestCommand2 : ICommand
    {
        public TestCommand2(Exception? exception) => Exception = exception;

        public Exception? Exception { get; }
    }
}