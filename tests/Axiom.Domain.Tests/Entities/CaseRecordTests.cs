using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using FluentAssertions;

namespace Axiom.Domain.Tests.Entities;

public class CaseRecordTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreate()
    {
        var record = new CaseRecord(
            new SystemName("CRM"),
            "Login failure",
            "Token expired",
            "Reset token",
            "Monitor refresh",
            new RitmId("RITM001"),
            "CHG001");

        record.Id.Should().NotBeEmpty();
        record.System.ToString().Should().Be("CRM");
        record.Problem.Should().Be("Login failure");
        record.Analysis.Should().Be("Token expired");
        record.Resolution.Should().Be("Reset token");
        record.LessonsLearned.Should().Be("Monitor refresh");
        record.RitmId.Should().Be(new RitmId("RITM001"));
        record.ChangeId.Should().Be("CHG001");
        record.Status.Should().Be(CaseStatus.Open);
    }

    [Fact]
    public void Constructor_WithEmptyProblem_ShouldThrow()
    {
        Action act = () => new CaseRecord(new SystemName("Sys"), "", null!, null!, null!);

        act.Should().Throw<ArgumentException>().WithParameterName("problem");
    }

    [Fact]
    public void UpdateStatus_ShouldChangeStatus()
    {
        var record = new CaseRecord(new SystemName("Sys"), "Problem", null!, null!, null!);

        record.UpdateStatus(CaseStatus.Resolved);

        record.Status.Should().Be(CaseStatus.Resolved);
    }
}
