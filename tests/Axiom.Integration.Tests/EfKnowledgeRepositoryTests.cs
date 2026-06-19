using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Integration.Tests;

public class EfKnowledgeRepositoryTests : IDisposable
{
    private readonly AxiomDbContext _context;
    private readonly IKnowledgeRepository _repo;
    private readonly ITagRepository _tagRepo;
    private readonly User _testUser;
    private readonly AxiomSystem _testSystem;
    private readonly KnowledgeType _testType;
    private readonly KnowledgeState _testState;

    public EfKnowledgeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AxiomDbContext>()
            .UseInMemoryDatabase($"axiom_test_{Guid.NewGuid()}")
            .Options;

        _context = new AxiomDbContext(options);
        _repo = new EfKnowledgeRepository(_context);
        _tagRepo = new EfTagRepository(_context);

        _testUser = new User("test@test.com", "Test User");
        _testSystem = new AxiomSystem("EAI001", "Test System", _testUser.UserId);
        _testType = new KnowledgeType("DOCS", "Documentation");
        _testState = new KnowledgeState("PUB", "Published");

        _context.Users.Add(_testUser);
        _context.Systems.Add(_testSystem);
        _context.KnowledgeTypes.Add(_testType);
        _context.KnowledgeStates.Add(_testState);
        _context.KnowledgeStates.Add(new KnowledgeState("DRF", "Draft"));
        _context.SaveChanges();
    }

    [Fact]
    public async Task ShouldRoundTripEntry()
    {
        var tag = await _tagRepo.FindOrCreateAsync("test-tag");

        var entry = new Knowledge(
            "Integration Test",
            "Integration Summary",
            "Integration Content",
            _testSystem.SystemId,
            _testUser.UserId,
            _testType.TypeId,
            _testState.StateId);

        entry.KnowledgeKnowledgeTags.Add(
            new KnowledgeKnowledgeTag(entry.KnowledgeId, tag.KnowledgeTagId));

        await _repo.SaveAsync(entry);

        var loaded = await _repo.GetByIdAsync(entry.KnowledgeId);

        loaded.Should().NotBeNull();
        loaded!.Title.Should().Be("Integration Test");
        loaded.Content.Should().Be("Integration Content");
        loaded.KnowledgeKnowledgeTags.Should().Contain(t => t.Tag != null && t.Tag.TagName == "test-tag");
    }

    [Fact]
    public async Task ShouldSearchEntries()
    {
        var entry = new Knowledge(
            "Searchable Entry",
            "Contains keyword",
            "Some content with keyword inside",
            _testSystem.SystemId,
            _testUser.UserId,
            _testType.TypeId,
            _testState.StateId);

        await _repo.SaveAsync(entry);

        var results = await _repo.SearchAsync("keyword");

        results.Should().Contain(e => e.KnowledgeId == entry.KnowledgeId);
    }

    [Fact]
    public async Task ShouldDeleteEntry()
    {
        var entry = new Knowledge(
            "To Delete", "summary", "content",
            _testSystem.SystemId, _testUser.UserId, _testType.TypeId, _testState.StateId);

        await _repo.SaveAsync(entry);
        await _repo.DeleteAsync(entry.KnowledgeId);

        var loaded = await _repo.GetByIdAsync(entry.KnowledgeId);
        loaded.Should().BeNull();
    }

    [Fact]
    public async Task ShouldUpdateExistingEntry()
    {
        var entry = new Knowledge(
            "Original Title", "summary", "content",
            _testSystem.SystemId, _testUser.UserId, _testType.TypeId, _testState.StateId);

        await _repo.SaveAsync(entry);

        entry.Update("Updated Title", "summary", "content", _testSystem.SystemId, _testType.TypeId, 2);
        await _repo.SaveAsync(entry);

        var loaded = await _repo.GetByIdAsync(entry.KnowledgeId);
        loaded.Should().NotBeNull();
        loaded!.Title.Should().Be("Updated Title");
        loaded.KnowledgeStateId.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
