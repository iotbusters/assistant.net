using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Assistant.Net.Storage.Sqlite.Tests
{
    public static class MockExtensions
    {
        /// <summary>
        ///   Verifies that a specific invocation matching the given expression was performed on the mock.
        /// </summary>
        /// <exception cref="ArgumentException" />
        public static void SimpleVerify<T>(this Mock<T> mock, Expression<Func<T, object>> expression, Func<Times> times) where T : class
        {
            if (expression.Body is not MethodCallExpression {Method: var method})
                throw new ArgumentException("Property or method call is expected only.", nameof(expression));

            var callValidator = times();
            var count = mock.Invocations.Count(x => x.Method == method);
            if (!callValidator.Validate(count))
            {
                callValidator.Deconstruct(out var from, out var to);
                Assert.Fail($"'{method.Name}' was expected tp be called from {from} to {to} times, but it was {count}");
            }
        }
    }
}
