using Axiom.Application.Interfaces;
using Axiom.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Integration.Tests;

public class EfReferenceDataServiceTests : IDisposable
{
    private readonly AxiomDbContext _context;
    private readonly IStartupService _startup;
    private readonly IReferenceDataService _references;

    public EfReferenceDataServiceTests()
    {
        var options = new DbContextOptionsBuilder<AxiomDbContext>()
            .UseInMemoryDatabase($"axiom_ref_test_{Guid.NewGuid()}")
            .Options;

        _context = new AxiomDbContext(options);
        _startup = new EfStartupService(_context);
        _references = new EfReferenceDataService(_context);
    }

    [Fact]
    public async Task ShouldListAndResolveReferenceData()
    {
        var user = await _startup.CreateUserAsync("ref@test.com", "Reference User", default);
        var system = await _startup.CreateSystemAsync("EAI-REF", "Reference System", user.UserId, default);
        var type = await _startup.CreateKnowledgeTypeAsync("REF", "Reference", default);
        var knowledgeState = await _startup.CreateKnowledgeStateAsync("PUB", "Published", default);
        var issueState = await _startup.CreateIssueStateAsync("OPEN", "Open", default);

        var users = await _references.ListUsersAsync();
        var systems = await _references.ListSystemsAsync();

        users.Should().ContainSingle(u => u.Email == "ref@test.com" && u.UserId == user.UserId);
        systems.Should().ContainSingle(s => s.EAI == "EAI-REF" && s.SystemId == system.SystemId);
        (await _references.FindUserByEmailAsync("ref@test.com"))!.UserId.Should().Be(user.UserId);
        (await _references.FindSystemByEaiAsync("EAI-REF"))!.SystemId.Should().Be(system.SystemId);
        (await _references.FindKnowledgeTypeByCodeAsync("REF"))!.Id.Should().Be(type.TypeId);
        (await _references.FindKnowledgeStateByCodeAsync("PUB"))!.Id.Should().Be(knowledgeState.StateId);
        (await _references.FindIssueStateByCodeAsync("OPEN"))!.Id.Should().Be(issueState.StateId);
        (await _references.FindUserByEmailAsync("missing@test.com")).Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
