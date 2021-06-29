using System;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using NUnit.Framework;
using Assistant.Net.Diagnostics.Abstractions;
using Assistant.Net.Diagnostics.Tests.Mocks;

namespace Assistant.Net.Diagnostics.Tests
{
    public class OperationFactoryTests
    {
        private ServiceProvider provider = null!;

        [OneTimeSetUp]
        public void Startup() =>
            provider = new ServiceCollection()
                .AddDiagnostics()
                .BuildServiceProvider();

        [OneTimeTearDown]
        public void TearDown() =>
            provider.Dispose();

        [Test]
        public void Dispose_Scope_OperationIncomplete()
        {
            using var eventListener = new TestOperationEventListener();
            var scope = provider.CreateScope();

            var correlationId = scope.ServiceProvider.GetRequiredService<IDiagnosticContext>().CorrelationId;
            var factory = scope.ServiceProvider.GetRequiredService<IDiagnosticFactory>();
            factory.Start("A"); // not disposed

            scope.Dispose();

            eventListener.EventPayloads.Should().BeEquivalentTo(new object[]
                {
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {correlationId, Array.Empty<object>(), Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {correlationId, Array.Empty<object>(), new TimeSpan(), "Operation wasn't properly stopped or lost.", "incomplete", Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Duration", "Message", "Status", "Metadata"}
                    }
                },
                opt => opt
                    .Using<TimeSpan>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(0.5)))
                    .WhenTypeIs<TimeSpan>());
        }

        [Test]
        public void Complete_TwoScopes_OperationsComplete()
        {
            using var eventListener = new TestOperationEventListener();

            var scope1 = provider.CreateScope();
            var correlationId1 = scope1.ServiceProvider.GetRequiredService<IDiagnosticContext>().CorrelationId;
            var factory1 = scope1.ServiceProvider.GetRequiredService<IDiagnosticFactory>();
            var operation1 = factory1.Start("A");

            var scope2 = provider.CreateScope();
            var correlationId2 = scope2.ServiceProvider.GetRequiredService<IDiagnosticContext>().CorrelationId;
            var factory2 = scope2.ServiceProvider.GetRequiredService<IDiagnosticFactory>();
            var operation2 = factory2.Start("B");
            operation2.Complete();
            scope2.Dispose();

            operation1.Complete();
            scope1.Dispose();

            eventListener.EventPayloads.Should().BeEquivalentTo(new object[]
                {
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {correlationId1, Array.Empty<object>(), Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {correlationId2, ItemData(correlationId1), Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {correlationId2, ItemData(correlationId1), new TimeSpan(), "Operation has successfully completed.", "complete", Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Duration", "Message", "Status", "Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] { correlationId1, Array.Empty<object>(), new TimeSpan(), "Operation has successfully completed.", "complete", Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Duration", "Message", "Status", "Metadata"}
                    }
                },
                opt => opt
                    .Using<TimeSpan>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(0.5)))
                    .WhenTypeIs<TimeSpan>());

        }

        [Test]
        public void Complete_HierarchicallySameNamedOperation_OperationsComplete()
        {
            using var eventListener = new TestOperationEventListener();
            using var scope = provider.CreateScope();

            var correlationId = scope.ServiceProvider.GetRequiredService<IDiagnosticContext>().CorrelationId;
            var factory = scope.ServiceProvider.GetRequiredService<IDiagnosticFactory>();
            var a1 = factory.Start("A");
            var a2 = factory.Start("A");
            a2.Complete();
            a1.Complete("test-message");

            scope.Dispose();

            eventListener.EventPayloads.Should().BeEquivalentTo(new object[]
                {
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {correlationId, Array.Empty<object>(), Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {correlationId, Array.Empty<object>(), Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {correlationId, Array.Empty<object>(), new TimeSpan(), "Operation has successfully completed.", "complete", Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Duration", "Message", "Status", "Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {correlationId, Array.Empty<object>(), new TimeSpan(), "test-message", "complete", Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Duration", "Message", "Status", "Metadata"}
                    }
                },
                opt => opt
                    .Using<TimeSpan>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(0.5)))
                    .WhenTypeIs<TimeSpan>());
        }

        [Test]
        public void Complete_Hierarchically_OperationsShareCorrelationId()
        {
            using var eventListener = new TestOperationEventListener();
            using var scope = provider.CreateScope();

            var correlationId = scope.ServiceProvider.GetRequiredService<IDiagnosticContext>().CorrelationId;
            var factory = scope.ServiceProvider.GetRequiredService<IDiagnosticFactory>();
            var a = factory.Start("A");
            var b = factory.Start("B");
            b.Complete();
            a.Complete();

            scope.Dispose();

            eventListener.EventPayloads.Should().BeEquivalentTo(new object[]
                {
                     new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {correlationId, Array.Empty<object>(), Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {correlationId, Array.Empty<object>(), Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {correlationId, Array.Empty<object>(), new TimeSpan(), "Operation has successfully completed.", "complete", Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Duration", "Message", "Status", "Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {correlationId, Array.Empty<object>(), new TimeSpan(), "Operation has successfully completed.", "complete", Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Duration", "Message", "Status", "Metadata"}
                    }
                },
                opt => opt
                    .Using<TimeSpan>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(0.5)))
                    .WhenTypeIs<TimeSpan>());
        }

        [Test]
        public void Complete_RootOperation_SubOperationIncomplete()
        {
            using var eventListener = new TestOperationEventListener();
            using var scope = provider.CreateScope();

            var correlationId = scope.ServiceProvider.GetRequiredService<IDiagnosticContext>().CorrelationId;
            var factory = scope.ServiceProvider.GetRequiredService<IDiagnosticFactory>();
            var a = factory.Start("A");
            var _ = factory.Start("B");
            a.Complete();

            scope.Dispose();

            //var settings = new JsonSerializerSettings {Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore, Converters = { new StringEnumConverter() }};
            //foreach (var a in eventListener.EventPayloads)
            //    Console.WriteLine(JsonConvert.SerializeObject(a, settings));

            eventListener.EventPayloads.Should().BeEquivalentTo(new object[]
                {
                     new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {correlationId, Array.Empty<object>(), Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {correlationId, Array.Empty<object>(), Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {correlationId, Array.Empty<object>(), new TimeSpan(), "Operation has successfully completed.", "complete", Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Duration", "Message", "Status", "Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = Guid.Empty,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {correlationId, Array.Empty<object>(), new TimeSpan(), "Operation wasn't properly stopped or lost.", "incomplete", Array.Empty<object>()},
                        PayloadNames = new[] {"CorrelationId", "ParentCorrelationIds", "Duration", "Message", "Status", "Metadata"}
                    }
                },
                opt => opt
                    .Using<TimeSpan>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(0.5)))
                    .WhenTypeIs<TimeSpan>());
        }

        private static object[] ItemData(string value) => new object[]
        {
            new {Keys = new[] {"Value"}, Values = new[] {value}}
        };
    }
}