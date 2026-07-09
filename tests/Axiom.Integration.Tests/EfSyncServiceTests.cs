using System.Text.Json;
using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Infrastructure.Persistence;
using Axiom.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Integration.Tests;

public class EfSyncServiceTests : IDisposable
{
    private readonly AxiomDbContext _context;
    private readonly JsonStore _store;
    private readonly string _storePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public EfSyncServiceTests()
    {
        _storePath = Path.Combine(Path.GetTempPath(), $"axiom_sync_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_storePath);

        var options = new DbContextOptionsBuilder<AxiomDbContext>()
            .UseInMemoryDatabase($"axiom_test_{Guid.NewGuid()}")
            .Options;

        _context = new AxiomDbContext(options);
        _store = new JsonStore(_storePath);
    }

    [Fact]
    public async Task SyncAsync_NoDataFiles_ShouldReturnTrue()
    {
        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysUpdate);

        var result = await service.SyncAsync(false, CancellationToken.None);

        result.Should().BeTrue();
        Directory.GetFiles(_storePath, "*.json").Should().BeEmpty();
    }

    [Fact]
    public async Task SyncAsync_NewUser_ShouldAddToDatabase()
    {
        await WriteStoreFile("users", new List<JsonUserEntry>
        {
            new() { UserId = Guid.NewGuid(), Email = "new@test.com", Name = "New User" }
        });

        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysUpdate);

        var result = await service.SyncAsync(false, CancellationToken.None);

        result.Should().BeTrue();
        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "new@test.com");
        dbUser.Should().NotBeNull();
        dbUser!.Name.Should().Be("New User");
    }

    [Fact]
    public async Task SyncAsync_UserConflictUpdate_ShouldModifyDatabase()
    {
        var existingId = Guid.NewGuid();
        _context.Users.Add(new User("existing@test.com", "Original Name"));
        await _context.SaveChangesAsync();
        var dbUser = await _context.Users.FirstAsync(u => u.Email == "existing@test.com");

        await WriteStoreFile("users", new List<JsonUserEntry>
        {
            new() { UserId = dbUser.UserId, Email = "existing@test.com", Name = "Modified Name" }
        });

        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysUpdate);

        await service.SyncAsync(false, CancellationToken.None);

        var reloaded = await _context.Users.FirstAsync(u => u.Email == "existing@test.com");
        reloaded.Name.Should().Be("Modified Name");
    }

    [Fact]
    public async Task SyncAsync_UserConflictSkip_ShouldKeepOriginal()
    {
        _context.Users.Add(new User("skip@test.com", "Original Name"));
        await _context.SaveChangesAsync();
        var dbUser = await _context.Users.FirstAsync(u => u.Email == "skip@test.com");

        await WriteStoreFile("users", new List<JsonUserEntry>
        {
            new() { UserId = dbUser.UserId, Email = "skip@test.com", Name = "Modified Name" }
        });

        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysSkip);

        await service.SyncAsync(false, CancellationToken.None);

        var reloaded = await _context.Users.FirstAsync(u => u.Email == "skip@test.com");
        reloaded.Name.Should().Be("Original Name");
    }

    [Fact]
    public async Task SyncAsync_NewSystem_ShouldAddToDatabase()
    {
        var user = new User("owner@test.com", "Owner");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await WriteStoreFile("systems", new List<JsonSystemEntry>
        {
            new() { SystemId = -1, EAI = "SYS001", Name = "Test System", OwnerUserId = user.UserId }
        });

        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysUpdate);

        await service.SyncAsync(false, CancellationToken.None);

        var dbSys = await _context.Systems.FirstOrDefaultAsync(s => s.EAI == "SYS001");
        dbSys.Should().NotBeNull();
        dbSys!.Name.Should().Be("Test System");
        dbSys.OwnerUserId.Should().Be(user.UserId);
    }

    [Fact]
    public async Task SyncAsync_SystemConflict_ShouldUpdate()
    {
        var user = new User("owner2@test.com", "Owner");
        _context.Users.Add(user);
        _context.Systems.Add(new AxiomSystem("SYS002", "Original Name", user.UserId));
        await _context.SaveChangesAsync();

        await WriteStoreFile("systems", new List<JsonSystemEntry>
        {
            new() { SystemId = -1, EAI = "SYS002", Name = "Modified Name", OwnerUserId = user.UserId }
        });

        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysUpdate);

        await service.SyncAsync(false, CancellationToken.None);

        var dbSys = await _context.Systems.FirstAsync(s => s.EAI == "SYS002");
        dbSys.Name.Should().Be("Modified Name");
    }

    [Fact]
    public async Task SyncAsync_ReferenceDataConflict_ShouldUpdate()
    {
        _context.KnowledgeTypes.Add(new KnowledgeType("GUIDE", "Original Guide"));
        _context.KnowledgeStates.Add(new KnowledgeState("DRAFT", "Original Draft"));
        _context.IssueStates.Add(new IssueState("OPEN", "Original Open"));
        _context.KnowledgeTags.Add(new KnowledgeTag("security"));
        await _context.SaveChangesAsync();

        await WriteStoreFile("knowledge-types", new List<JsonKnowledgeTypeEntry>
        {
            new() { TypeId = -1, Code = "GUIDE", Name = "Modified Guide" }
        });
        await WriteStoreFile("knowledge-states", new List<JsonKnowledgeStateEntry>
        {
            new() { StateId = -1, Code = "DRAFT", Name = "Modified Draft" }
        });
        await WriteStoreFile("issue-states", new List<JsonIssueStateEntry>
        {
            new() { StateId = -1, Code = "OPEN", Name = "Modified Open" }
        });
        await WriteStoreFile("knowledge-tags", new List<JsonKnowledgeTagEntry>
        {
            new() { KnowledgeTagId = -1, TagName = "security" }
        });

        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysUpdate);

        await service.SyncAsync(false, CancellationToken.None);

        (await _context.KnowledgeTypes.FirstAsync(t => t.Code == "GUIDE")).Name.Should().Be("Modified Guide");
        (await _context.KnowledgeStates.FirstAsync(s => s.Code == "DRAFT")).Name.Should().Be("Modified Draft");
        (await _context.IssueStates.FirstAsync(s => s.Code == "OPEN")).Name.Should().Be("Modified Open");
    }

    [Fact]
    public async Task SyncAsync_NewComponent_ShouldRemapSystemId()
    {
        var user = new User("comp@test.com", "Owner");
        _context.Users.Add(user);
        _context.Systems.Add(new AxiomSystem("SYS010", "System Ten", user.UserId));
        await _context.SaveChangesAsync();
        var dbSys = await _context.Systems.FirstAsync(s => s.EAI == "SYS010");

        await WriteStoreFile("systems", new List<JsonSystemEntry>
        {
            new() { SystemId = -1, EAI = "SYS010", Name = "System Ten", OwnerUserId = user.UserId }
        });
        await WriteStoreFile("components", new List<JsonTechnicalComponentEntry>
        {
            new()
            {
                ComponentId = Guid.NewGuid(),
                Name = "Test API",
                TechnicalName = "test-api",
                ComponentType = "Api",
                Environment = "PROD",
                Criticality = "High",
                Description = "A test API",
                SystemId = -1
            }
        });

        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysUpdate);

        await service.SyncAsync(false, CancellationToken.None);

        var dbComp = await _context.TechnicalComponents.FirstOrDefaultAsync(c => c.Name == "Test API");
        dbComp.Should().NotBeNull();
        dbComp!.SystemId.Should().Be(dbSys.SystemId);
    }

    [Fact]
    public async Task SyncAsync_IssueNew_ShouldRemapSystemAndState()
    {
        var user = new User("issue@test.com", "Owner");
        _context.Users.Add(user);
        _context.Systems.Add(new AxiomSystem("SYS020", "System Twenty", user.UserId));
        _context.IssueStates.Add(new IssueState("OPEN", "Open"));
        await _context.SaveChangesAsync();

        await WriteStoreFile("issue-states", new List<JsonIssueStateEntry>
        {
            new() { StateId = -1, Code = "OPEN", Name = "Open" }
        });
        await WriteStoreFile("systems", new List<JsonSystemEntry>
        {
            new() { SystemId = -1, EAI = "SYS020", Name = "System Twenty", OwnerUserId = user.UserId }
        });
        await WriteStoreFile("issues", new List<JsonIssueEntry>
        {
            new()
            {
                IssueId = Guid.NewGuid(),
                Summary = "Test Issue",
                Problem = "Something broke",
                SystemId = -1,
                StateId = -1,
                CreatedByUserId = user.UserId
            }
        });

        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysUpdate);

        await service.SyncAsync(false, CancellationToken.None);

        var dbIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Summary == "Test Issue");
        dbIssue.Should().NotBeNull();
    }

    [Fact]
    public async Task SyncAsync_FullCycle_ShouldRemapAllForeignKeys()
    {
        var user = new User("full@test.com", "Full User");
        _context.Users.Add(user);
        _context.Systems.Add(new AxiomSystem("SYS030", "System Thirty", user.UserId));
        _context.KnowledgeTypes.Add(new KnowledgeType("GUIDE", "Guide"));
        _context.KnowledgeStates.Add(new KnowledgeState("DRAFT", "Draft"));
        _context.IssueStates.Add(new IssueState("OPEN", "Open"));
        _context.KnowledgeTags.Add(new KnowledgeTag("security"));
        await _context.SaveChangesAsync();

        var dbUser = await _context.Users.FirstAsync(u => u.Email == "full@test.com");
        var dbSys = await _context.Systems.FirstAsync(s => s.EAI == "SYS030");
        var dbType = await _context.KnowledgeTypes.FirstAsync(t => t.Code == "GUIDE");
        var dbState = await _context.KnowledgeStates.FirstAsync(s => s.Code == "DRAFT");
        var dbIssueState = await _context.IssueStates.FirstAsync(s => s.Code == "OPEN");
        var dbTag = await _context.KnowledgeTags.FirstAsync(t => t.TagName == "security");

        var issueId = Guid.NewGuid();
        var compSourceId = Guid.NewGuid();
        var compTargetId = Guid.NewGuid();
        var depId = Guid.NewGuid();
        var traceId = Guid.NewGuid();
        var knowledgeId = Guid.NewGuid();

        await WriteStoreFile("knowledge-types", new List<JsonKnowledgeTypeEntry>
        {
            new() { TypeId = -1, Code = "GUIDE", Name = "Guide" }
        });
        await WriteStoreFile("knowledge-states", new List<JsonKnowledgeStateEntry>
        {
            new() { StateId = -1, Code = "DRAFT", Name = "Draft" }
        });
        await WriteStoreFile("issue-states", new List<JsonIssueStateEntry>
        {
            new() { StateId = -1, Code = "OPEN", Name = "Open" }
        });
        await WriteStoreFile("knowledge-tags", new List<JsonKnowledgeTagEntry>
        {
            new() { KnowledgeTagId = -1, TagName = "security" }
        });
        await WriteStoreFile("systems", new List<JsonSystemEntry>
        {
            new() { SystemId = -1, EAI = "SYS030", Name = "System Thirty", OwnerUserId = dbUser.UserId }
        });
        await WriteStoreFile("components", new List<JsonTechnicalComponentEntry>
        {
            new()
            {
                ComponentId = compSourceId,
                Name = "Payment API",
                TechnicalName = "payment-api",
                ComponentType = "Api",
                Environment = "PROD",
                Criticality = "Critical",
                Description = "Handles payments",
                SystemId = -1
            },
            new()
            {
                ComponentId = compTargetId,
                Name = "Payment DB",
                TechnicalName = "payment-db",
                ComponentType = "Database",
                Environment = "PROD",
                Criticality = "Critical",
                Description = "Database",
                SystemId = -1
            }
        });
        await WriteStoreFile("issues", new List<JsonIssueEntry>
        {
            new()
            {
                IssueId = issueId,
                Summary = "Payment Timeout",
                Problem = "Timeout error",
                SystemId = -1,
                StateId = -1,
                CreatedByUserId = dbUser.UserId,
                RitmNumber = "RITM-001",
                IncidentNumber = "INC-001"
            }
        });
        await WriteStoreFile("dependencies", new List<JsonComponentDependencyEntry>
        {
            new()
            {
                DependencyId = depId,
                SourceComponentId = compSourceId,
                TargetComponentId = compTargetId,
                DependencyType = "WritesTo",
                Criticality = "High",
                Status = "Active",
                Description = "Payment API writes to Payment DB"
            }
        });
        await WriteStoreFile("trace-events", new List<JsonDependencyTraceEventEntry>
        {
            new()
            {
                TraceEventId = traceId,
                DependencyId = depId,
                EventType = "Updated",
                Description = "Updated configuration",
                CreatedByUserId = dbUser.UserId
            }
        });
        await WriteStoreFile("knowledge", new List<JsonKnowledgeEntry>
        {
            new()
            {
                KnowledgeId = knowledgeId,
                Title = "Payment Guide",
                Summary = "How to fix payments",
                Content = "Step by step guide",
                SystemId = -1,
                CreatedByUserId = dbUser.UserId,
                KnowledgeTypeId = -1,
                KnowledgeStateId = -1,
                Tags = new List<string> { "security" }
            }
        });

        var service = new EfSyncService(_context, _store);
        service.SetConflictResolver(AlwaysUpdate);

        var syncResult = await service.SyncAsync(false, CancellationToken.None);
        syncResult.Should().BeTrue();

        var dbComp = await _context.TechnicalComponents.FirstAsync(c => c.ComponentId == compSourceId);
        dbComp.SystemId.Should().Be(dbSys.SystemId);

        var dbIssue = await _context.Issues.FirstOrDefaultAsync(i => i.IssueId == issueId);
        dbIssue.Should().NotBeNull();
        dbIssue!.SystemId.Should().Be(dbSys.SystemId);

        var dbDep = await _context.ComponentDependencies.FirstOrDefaultAsync(d => d.DependencyId == depId);
        dbDep.Should().NotBeNull();
        dbDep!.DependencyId.Should().Be(depId);

        var dbTrace = await _context.DependencyTraceEvents.FirstOrDefaultAsync(e => e.TraceEventId == traceId);
        if (dbTrace is null)
        {
            var allTraces = await _context.DependencyTraceEvents.ToListAsync();
            Assert.Fail($"Trace event not found. Total traces in DB: {allTraces.Count}");
        }
        dbTrace.Description.Should().Be("Updated configuration");

        var dbKnowledge = await _context.Knowledges
            .Include(k => k.KnowledgeKnowledgeTags)
            .FirstAsync(k => k.KnowledgeId == knowledgeId);
        dbKnowledge.Title.Should().Be("Payment Guide");
        dbKnowledge.KnowledgeKnowledgeTags.Should().Contain(t => t.KnowledgeTagId == dbTag.KnowledgeTagId);
    }

    [Fact]
    public async Task SyncAsync_DryRun_ShouldNotModifyDatabase()
    {
        await WriteStoreFile("users", new List<JsonUserEntry>
        {
            new() { UserId = Guid.NewGuid(), Email = "dry@test.com", Name = "Dry Run User" }
        });

        var service = new EfSyncService(_context, _store);

        await service.SyncAsync(true, CancellationToken.None);

        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "dry@test.com");
        dbUser.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
        if (Directory.Exists(_storePath))
            Directory.Delete(_storePath, recursive: true);
    }

    private static ConflictAction AlwaysUpdate(SyncConflictInfo _) => ConflictAction.AlwaysUpdate;
    private static ConflictAction AlwaysSkip(SyncConflictInfo _) => ConflictAction.AlwaysSkip;

    private async Task WriteStoreFile<T>(string entityName, List<T> entries)
    {
        var path = Path.Combine(_storePath, $"{entityName}.json");
        var json = JsonSerializer.Serialize(entries, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }
}
