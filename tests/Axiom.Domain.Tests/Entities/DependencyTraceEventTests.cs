using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using FluentAssertions;

namespace Axiom.Domain.Tests.Entities;

public class DependencyTraceEventTests
{
    private readonly Guid _dependencyId = Guid.NewGuid();
    private readonly Guid _traceEventId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreate()
    {
        var ev = new DependencyTraceEvent(
            _dependencyId,
            DependencyTraceEventType.Created,
            "Dependency created",
            traceEventId: _traceEventId);

        ev.TraceEventId.Should().Be(_traceEventId);
        ev.DependencyId.Should().Be(_dependencyId);
        ev.EventType.Should().Be(DependencyTraceEventType.Created);
        ev.Description.Should().Be("Dependency created");
        ev.RelatedIssueId.Should().BeNull();
        ev.RelatedKnowledgeId.Should().BeNull();
        ev.RelatedRitmNumber.Should().BeNull();
        ev.RelatedChangeNumber.Should().BeNull();
        ev.CreatedByUserId.Should().BeNull();
        ev.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithAllOptionalFields_ShouldCreate()
    {
        var issueId = Guid.NewGuid();
        var knowledgeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var ev = new DependencyTraceEvent(
            _dependencyId,
            DependencyTraceEventType.IssueLinked,
            "Linked to issue INC-123",
            relatedIssueId: issueId,
            relatedKnowledgeId: knowledgeId,
            relatedRitmNumber: "RITM001",
            relatedChangeNumber: "CHG001",
            createdByUserId: userId);

        ev.RelatedIssueId.Should().Be(issueId);
        ev.RelatedKnowledgeId.Should().Be(knowledgeId);
        ev.RelatedRitmNumber.Should().Be("RITM001");
        ev.RelatedChangeNumber.Should().Be("CHG001");
        ev.CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public void Constructor_WithoutTraceEventId_ShouldGenerateGuid()
    {
        var ev = new DependencyTraceEvent(_dependencyId, DependencyTraceEventType.Validated, "Validated");
        ev.TraceEventId.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyDescription_ShouldThrow()
    {
        Action act = () => new DependencyTraceEvent(_dependencyId, DependencyTraceEventType.NoteAdded, "");
        act.Should().Throw<ArgumentException>().WithParameterName("description");
    }

    [Fact]
    public void Constructor_WithAllEventTypes_ShouldCreate()
    {
        var types = new[]
        {
            DependencyTraceEventType.Created,
            DependencyTraceEventType.Updated,
            DependencyTraceEventType.Validated,
            DependencyTraceEventType.Failed,
            DependencyTraceEventType.Deprecated,
            DependencyTraceEventType.IssueLinked,
            DependencyTraceEventType.KnowledgeLinked,
            DependencyTraceEventType.RitmLinked,
            DependencyTraceEventType.ChangeLinked,
            DependencyTraceEventType.NoteAdded,
            DependencyTraceEventType.Unknown
        };

        foreach (var type in types)
        {
            var ev = new DependencyTraceEvent(_dependencyId, type, $"Event {type}");
            ev.EventType.Should().Be(type);
        }
    }
}
