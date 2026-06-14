using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using Axiom.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Axiom.Integration.Tests;

public class JsonRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IKnowledgeRepository _knowledgeRepo;
    private readonly ICaseRepository _caseRepo;

    public JsonRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"axiom_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        var options = Substitute.For<IOptions<JsonDataOptions>>();
        options.Value.Returns(new JsonDataOptions
        {
            KnowledgeFilePath = Path.Combine(_tempDir, "knowledge.json"),
            CasesFilePath = Path.Combine(_tempDir, "cases.json")
        });

        var loggerKnowledge = Substitute.For<ILogger<JsonKnowledgeRepository>>();
        var loggerCase = Substitute.For<ILogger<JsonCaseRepository>>();

        _knowledgeRepo = new JsonKnowledgeRepository(options, loggerKnowledge);
        _caseRepo = new JsonCaseRepository(options, loggerCase);
    }

    [Fact]
    public async Task KnowledgeRepository_ShouldRoundTripEntry()
    {
        var entry = new KnowledgeEntry(
            "Integration Test",
            "Integration Description",
            "Integration Content",
            new SystemName("TestSys"),
            ["integ", "test"],
            "TestAuthor",
            KnowledgeType.Documentation,
            KnowledgeStatusValue.Draft);

        await _knowledgeRepo.SaveAsync(entry);

        var loaded = await _knowledgeRepo.GetByIdAsync(entry.Id);

        loaded.Should().NotBeNull();
        loaded!.Title.Should().Be(entry.Title);
        loaded.Content.Should().Be(entry.Content);
        loaded.System.ToString().Should().Be("TestSys");
        loaded.Tags.Should().Contain(["integ", "test"]);
    }

    [Fact]
    public async Task KnowledgeRepository_ShouldSearchEntries()
    {
        var entry = new KnowledgeEntry(
            "Searchable Entry",
            "Contains keyword",
            "Some content with keyword inside",
            new SystemName("Sys"), ["tag"],
            "author", KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        await _knowledgeRepo.SaveAsync(entry);

        var results = await _knowledgeRepo.SearchAsync("keyword");

        results.Should().Contain(e => e.Id == entry.Id);
    }

    [Fact]
    public async Task CaseRepository_ShouldRoundTripRecord()
    {
        var record = new CaseRecord(
            new SystemName("CRM"),
            "Login issue",
            "Token analysis",
            "Reset password",
            "Document process",
            new RitmId("RITM999"),
            "CHG888");

        await _caseRepo.SaveAsync(record);

        var loaded = await _caseRepo.GetByIdAsync(record.Id);

        loaded.Should().NotBeNull();
        loaded!.Problem.Should().Be("Login issue");
        loaded.System.ToString().Should().Be("CRM");
        loaded.RitmId.Should().Be(new RitmId("RITM999"));
        loaded.ChangeId.Should().Be("CHG888");
    }

    [Fact]
    public async Task KnowledgeRepository_ShouldDeleteEntry()
    {
        var entry = new KnowledgeEntry(
            "To Delete", "desc", "content",
            new SystemName("Sys"), [], "author",
            KnowledgeType.Documentation, KnowledgeStatusValue.Draft);

        await _knowledgeRepo.SaveAsync(entry);
        await _knowledgeRepo.DeleteAsync(entry.Id);

        var loaded = await _knowledgeRepo.GetByIdAsync(entry.Id);
        loaded.Should().BeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }
}
