using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Integration.Tests;

public class EfStartupServiceTests : IDisposable
{
    private readonly AxiomDbContext _context;
    private readonly IStartupService _svc;

    public EfStartupServiceTests()
    {
        var options = new DbContextOptionsBuilder<AxiomDbContext>()
            .UseInMemoryDatabase($"axiom_test_{Guid.NewGuid()}")
            .Options;

        _context = new AxiomDbContext(options);
        _svc = new EfStartupService(_context);
    }

    [Fact]
    public async Task ShouldCreateUser()
    {
        var user = await _svc.CreateUserAsync("alice@test.com", "Alice", default);

        user.UserId.Should().NotBeEmpty();
        user.Email.Should().Be("alice@test.com");
        user.Name.Should().Be("Alice");

        var loaded = await _context.Users.FindAsync(user.UserId);
        loaded.Should().NotBeNull();
        loaded!.Email.Should().Be("alice@test.com");
    }

    [Fact]
    public async Task ShouldCreateSystem()
    {
        var owner = await _svc.CreateUserAsync("owner@test.com", "Owner", default);

        var system = await _svc.CreateSystemAsync("EAI001", "Core API", owner.UserId, default);

        system.SystemId.Should().BeGreaterThan(0);
        system.EAI.Should().Be("EAI001");
        system.Name.Should().Be("Core API");
        system.OwnerUserId.Should().Be(owner.UserId);

        var loaded = await _context.Systems.FindAsync(system.SystemId);
        loaded.Should().NotBeNull();
        loaded!.EAI.Should().Be("EAI001");
    }

    [Fact]
    public async Task ShouldCreateKnowledgeType()
    {
        var kt = await _svc.CreateKnowledgeTypeAsync("DOCS", "Documentation", default);

        kt.TypeId.Should().BeGreaterThan(0);
        kt.Code.Should().Be("DOCS");
        kt.Name.Should().Be("Documentation");

        var loaded = await _context.KnowledgeTypes.FindAsync(kt.TypeId);
        loaded.Should().NotBeNull();
        loaded!.Code.Should().Be("DOCS");
    }

    [Fact]
    public async Task ShouldCreateIssueState()
    {
        var state = await _svc.CreateIssueStateAsync("OPEN", "Open", default);

        state.StateId.Should().BeGreaterThan(0);
        state.Code.Should().Be("OPEN");
        state.Name.Should().Be("Open");

        var loaded = await _context.IssueStates.FindAsync(state.StateId);
        loaded.Should().NotBeNull();
        loaded!.Code.Should().Be("OPEN");
    }

    [Fact]
    public async Task ShouldCreateKnowledgeState()
    {
        var state = await _svc.CreateKnowledgeStateAsync("PUB", "Published", default);

        state.StateId.Should().BeGreaterThan(0);
        state.Code.Should().Be("PUB");
        state.Name.Should().Be("Published");

        var loaded = await _context.KnowledgeStates.FindAsync(state.StateId);
        loaded.Should().NotBeNull();
        loaded!.Code.Should().Be("PUB");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
