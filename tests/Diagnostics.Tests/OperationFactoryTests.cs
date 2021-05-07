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

        [SetUp]
        public void Startup() =>
            provider = new ServiceCollection()
                .AddDiagnostics()
                .BuildServiceProvider();

        public void TearDown() =>
            provider.Dispose();

        [Test]
        public void Dispose_Scope_OperationIncomplete()
        {
            using var eventListener = new TestOperationEventListener();
            var scope = provider.CreateScope();

            var correlationId = scope.ServiceProvider.GetRequiredService<IDiagnosticsContext>().CorrelationId;
            var factory = scope.ServiceProvider.GetRequiredService<IDiagnosticsFactory>();
            factory.Start("A"); // not disposed

            scope.Dispose();

            eventListener.EventPayloads.Should().BeEquivalentTo(new object[]
            {
                new
                {
                    EventName = "A",
                    ActivityId = Guid.Empty,
                    RelatedActivityId = correlationId,
                    EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                    Opcode = EventOpcode.Start,
                    Payload = new object[] {CorrelationIdPayload(correlationId)},
                    PayloadNames = new[] {"Metadata"}
                },
                new
                {
                    EventName = "A",
                    ActivityId = Guid.Empty,
                    RelatedActivityId = correlationId,
                    EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                    Opcode = EventOpcode.Stop,
                    Payload = new object[] {new TimeSpan(), "Operation wasn't properly stopped or lost.", "incomplete", CorrelationIdPayload(correlationId)},
                    PayloadNames = new[] {"Duration", "Message", "Status", "Metadata"}
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
            var correlationId1 = scope1.ServiceProvider.GetRequiredService<IDiagnosticsContext>().CorrelationId;
            var factory1 = scope1.ServiceProvider.GetRequiredService<IDiagnosticsFactory>();
            var operation1 = factory1.Start("A");

            var scope2 = provider.CreateScope();
            var correlationId2 = scope2.ServiceProvider.GetRequiredService<IDiagnosticsContext>().CorrelationId;
            var factory2 = scope2.ServiceProvider.GetRequiredService<IDiagnosticsFactory>();
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
                    RelatedActivityId = correlationId1,
                    EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                    Opcode = EventOpcode.Start,
                    Payload = new[] {CorrelationIdPayload(correlationId1)},
                    PayloadNames = new[] {"Metadata"}
                },
                new
                {
                    EventName = "B",
                    ActivityId = Guid.Empty,
                    RelatedActivityId = correlationId2,
                    EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                    Opcode = EventOpcode.Start,
                    Payload = new object[] {CorrelationIdPayload(correlationId2, correlationId1)},
                    PayloadNames = new[] {"Metadata"}
                },
                new
                {
                    EventName = "B",
                    ActivityId = Guid.Empty,
                    RelatedActivityId = correlationId2,
                    EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                    Opcode = EventOpcode.Stop,
                    Payload = new object[] {new TimeSpan(), "Operation has successfully completed.", "complete", CorrelationIdPayload(correlationId2, correlationId1)},
                    PayloadNames = new[] {"Duration", "Message", "Status", "Metadata"}
                },
                new
                {
                    EventName = "A",
                    ActivityId = Guid.Empty,
                    RelatedActivityId = correlationId1,
                    EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                    Opcode = EventOpcode.Stop,
                    Payload = new object[] { new TimeSpan(), "Operation has successfully completed.", "complete", CorrelationIdPayload(correlationId1)},
                    PayloadNames = new[] {"Duration", "Message", "Status", "Metadata"}
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

            var correlationId = scope.ServiceProvider.GetRequiredService<IDiagnosticsContext>().CorrelationId;
            var factory = scope.ServiceProvider.GetRequiredService<IDiagnosticsFactory>();
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
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {new TimeSpan(), "Operation has successfully completed.", "complete", CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Duration", "Message", "Status", "Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {new TimeSpan(), "test-message", "complete", CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Duration", "Message", "Status", "Metadata"}
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

            var correlationId = scope.ServiceProvider.GetRequiredService<IDiagnosticsContext>().CorrelationId;
            var factory = scope.ServiceProvider.GetRequiredService<IDiagnosticsFactory>();
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
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {new TimeSpan(), "Operation has successfully completed.", "complete", CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Duration", "Message", "Status", "Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {new TimeSpan(), "Operation has successfully completed.", "complete", CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Duration", "Message", "Status", "Metadata"}
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

            var correlationId = scope.ServiceProvider.GetRequiredService<IDiagnosticsContext>().CorrelationId;
            var factory = scope.ServiceProvider.GetRequiredService<IDiagnosticsFactory>();
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
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Start,
                        Payload = new object[] {CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Metadata"}
                    },
                    new
                    {
                        EventName = "A",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {new TimeSpan(), "Operation has successfully completed.", "complete", CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Duration", "Message", "Status", "Metadata"}
                    },
                    new
                    {
                        EventName = "B",
                        ActivityId = Guid.Empty,
                        RelatedActivityId = correlationId,
                        EventSource = new {Name = "Assistant.Net.Diagnostics.Operation"},
                        Opcode = EventOpcode.Stop,
                        Payload = new object[] {new TimeSpan(), "Operation wasn't properly stopped or lost.", "incomplete", CorrelationIdPayload(correlationId)},
                        PayloadNames = new[] {"Duration", "Message", "Status", "Metadata"}
                    }
                },
                opt => opt
                    .Using<TimeSpan>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(0.5)))
                    .WhenTypeIs<TimeSpan>());
        }

        private static object[] CorrelationIdPayload(params Guid[] correlationIds) => new object[]
        {
            new {Keys = new[] {"Key", "Value"}, Values = new[] {"correlation-id", string.Join(",", correlationIds)}}
        };
    }
}