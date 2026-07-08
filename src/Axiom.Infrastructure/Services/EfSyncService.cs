using System.Text.Json;
using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Domain.Entities;
using Axiom.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Axiom.Infrastructure.Services;

public class EfSyncService : ISyncService
{
    private readonly AxiomDbContext _db;
    private readonly IJsonStore _store;
    private Func<SyncConflictInfo, ConflictAction>? _conflictResolver;
    private bool _dryRun;

    private readonly Dictionary<long, long> _systemIdMap = new();
    private readonly Dictionary<long, long> _typeIdMap = new();
    private readonly Dictionary<int, int> _ksIdMap = new();
    private readonly Dictionary<int, int> _isIdMap = new();
    private readonly Dictionary<long, long> _tagIdMap = new();
    private readonly Dictionary<Guid, Guid> _issueIdMap = new();
    private readonly Dictionary<Guid, Guid> _componentIdMap = new();
    private readonly Dictionary<Guid, Guid> _dependencyIdMap = new();

    public EfSyncService(AxiomDbContext db, IJsonStore store)
    {
        _db = db;
        _store = store;
    }

    public async Task<bool> SyncAsync(bool dryRun, CancellationToken ct)
    {
        _dryRun = dryRun;

        Console.WriteLine(dryRun
            ? "DRY RUN mode - no changes will be made"
            : "Starting sync...");

        _conflictResolver = dryRun
            ? (info) => { Console.WriteLine($"  Conflict detected ({info.EntityType}: {info.Identifier}) — DRY RUN, would prompt"); return ConflictAction.Skip; }
            : null;

        try
        {
            await SyncUsers(ct);
            await SyncKnowledgeTypes(ct);
            await SyncKnowledgeStates(ct);
            await SyncIssueStates(ct);
            await SyncKnowledgeTags(ct);
            await SyncSystems(ct);
            await SyncTechnicalComponents(ct);
            await SyncIssues(ct);
            await SyncComponentDependencies(ct);
            await SyncDependencyTraceEvents(ct);
            await SyncKnowledges(ct);

            if (!_dryRun)
            {
                await ArchiveSyncedFiles(ct);
                Console.WriteLine("Sync completed successfully. Files archived.");
            }
            else
            {
                Console.WriteLine("DRY RUN completed. No files were archived.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sync failed: {ex.Message}");
            return false;
        }
    }

    public void SetConflictResolver(Func<SyncConflictInfo, ConflictAction> resolver)
    {
        _conflictResolver = resolver;
    }

    private async Task SyncUsers(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonUserEntry>("users", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} users...");
        var existingUsers = await _db.Users.AsNoTracking().ToListAsync(ct);
        var existingEmails = new HashSet<string>(existingUsers.Select(u => u.Email), StringComparer.OrdinalIgnoreCase);
        var dbUsersByEmail = existingUsers.ToDictionary(u => u.Email, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            if (existingEmails.Contains(entry.Email))
            {
                var dbUser = dbUsersByEmail[entry.Email];
                var action = ResolveConflict("User", entry.Email, entry, dbUser);
                if (action is ConflictAction.Skip or ConflictAction.AlwaysSkip) continue;
                if (action is ConflictAction.Update or ConflictAction.AlwaysUpdate)
                {
                    if (!_dryRun)
                    {
                        var user = await _db.Users.FindAsync([dbUser.UserId], ct);
                        user?.Update(entry.Email, entry.Name);
                    }
                }
            }
            else
            {
                if (!_dryRun)
                {
                    var user = new User(entry.Email, entry.Name);
                    _db.Users.Add(user);
                }
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncKnowledgeTypes(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonKnowledgeTypeEntry>("knowledge-types", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} knowledge types...");
        var existing = await _db.KnowledgeTypes.AsNoTracking().ToListAsync(ct);
        var existingCodes = new HashSet<string>(existing.Select(t => t.Code), StringComparer.OrdinalIgnoreCase);
        var dbByCode = existing.ToDictionary(t => t.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            if (existingCodes.Contains(entry.Code))
            {
                var dbType = dbByCode[entry.Code];
                var action = ResolveConflict("KnowledgeType", entry.Code, entry, dbType);
                if (action is ConflictAction.Skip or ConflictAction.AlwaysSkip)
                {
                    _typeIdMap[entry.TypeId] = dbType.TypeId;
                    continue;
                }
                if (action is ConflictAction.Update or ConflictAction.AlwaysUpdate)
                {
                    if (!_dryRun)
                    {
                        var type = await _db.KnowledgeTypes.FindAsync([dbType.TypeId], ct);
                        type?.Update(entry.Code, entry.Name);
                    }
                    _typeIdMap[entry.TypeId] = dbType.TypeId;
                }
            }
            else
            {
                if (!_dryRun)
                {
                    var type = new KnowledgeType(entry.Code, entry.Name);
                    _db.KnowledgeTypes.Add(type);
                    await _db.SaveChangesAsync(ct);
                    _typeIdMap[entry.TypeId] = type.TypeId;
                }
                else
                {
                    _typeIdMap[entry.TypeId] = entry.TypeId;
                }
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncKnowledgeStates(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonKnowledgeStateEntry>("knowledge-states", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} knowledge states...");
        var existing = await _db.KnowledgeStates.AsNoTracking().ToListAsync(ct);
        var existingCodes = new HashSet<string>(existing.Select(s => s.Code), StringComparer.OrdinalIgnoreCase);
        var dbByCode = existing.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            if (existingCodes.Contains(entry.Code))
            {
                var dbState = dbByCode[entry.Code];
                var action = ResolveConflict("KnowledgeState", entry.Code, entry, dbState);
                if (action is ConflictAction.Skip or ConflictAction.AlwaysSkip)
                {
                    _ksIdMap[entry.StateId] = dbState.StateId;
                    continue;
                }
                if (action is ConflictAction.Update or ConflictAction.AlwaysUpdate)
                {
                    if (!_dryRun)
                    {
                        var state = await _db.KnowledgeStates.FindAsync([dbState.StateId], ct);
                        state?.Update(entry.Code, entry.Name);
                    }
                    _ksIdMap[entry.StateId] = dbState.StateId;
                }
            }
            else
            {
                if (!_dryRun)
                {
                    var state = new KnowledgeState(entry.Code, entry.Name);
                    _db.KnowledgeStates.Add(state);
                    await _db.SaveChangesAsync(ct);
                    _ksIdMap[entry.StateId] = state.StateId;
                }
                else
                {
                    _ksIdMap[entry.StateId] = entry.StateId;
                }
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncIssueStates(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonIssueStateEntry>("issue-states", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} issue states...");
        var existing = await _db.IssueStates.AsNoTracking().ToListAsync(ct);
        var existingCodes = new HashSet<string>(existing.Select(s => s.Code), StringComparer.OrdinalIgnoreCase);
        var dbByCode = existing.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            if (existingCodes.Contains(entry.Code))
            {
                var dbState = dbByCode[entry.Code];
                var action = ResolveConflict("IssueState", entry.Code, entry, dbState);
                if (action is ConflictAction.Skip or ConflictAction.AlwaysSkip)
                {
                    _isIdMap[entry.StateId] = dbState.StateId;
                    continue;
                }
                if (action is ConflictAction.Update or ConflictAction.AlwaysUpdate)
                {
                    if (!_dryRun)
                    {
                        var state = await _db.IssueStates.FindAsync([dbState.StateId], ct);
                        state?.Update(entry.Code, entry.Name);
                    }
                    _isIdMap[entry.StateId] = dbState.StateId;
                }
            }
            else
            {
                if (!_dryRun)
                {
                    var state = new IssueState(entry.Code, entry.Name);
                    _db.IssueStates.Add(state);
                    await _db.SaveChangesAsync(ct);
                    _isIdMap[entry.StateId] = state.StateId;
                }
                else
                {
                    _isIdMap[entry.StateId] = entry.StateId;
                }
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncKnowledgeTags(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonKnowledgeTagEntry>("knowledge-tags", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} knowledge tags...");
        var existing = await _db.KnowledgeTags.AsNoTracking().ToListAsync(ct);
        var existingNames = new HashSet<string>(existing.Select(t => t.TagName), StringComparer.OrdinalIgnoreCase);
        var dbByName = existing.ToDictionary(t => t.TagName, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            if (existingNames.Contains(entry.TagName))
            {
                var dbTag = dbByName[entry.TagName];
                var action = ResolveConflict("KnowledgeTag", entry.TagName, entry, dbTag);
                if (action is ConflictAction.Skip or ConflictAction.AlwaysSkip)
                {
                    _tagIdMap[entry.KnowledgeTagId] = dbTag.KnowledgeTagId;
                    continue;
                }
                if (action is ConflictAction.Update or ConflictAction.AlwaysUpdate)
                {
                    if (!_dryRun)
                    {
                        var tag = await _db.KnowledgeTags.FindAsync([dbTag.KnowledgeTagId], ct);
                        tag?.Update(entry.TagName);
                    }
                    _tagIdMap[entry.KnowledgeTagId] = dbTag.KnowledgeTagId;
                }
            }
            else
            {
                if (!_dryRun)
                {
                    var tag = new KnowledgeTag(entry.TagName);
                    _db.KnowledgeTags.Add(tag);
                    await _db.SaveChangesAsync(ct);
                    _tagIdMap[entry.KnowledgeTagId] = tag.KnowledgeTagId;
                }
                else
                {
                    _tagIdMap[entry.KnowledgeTagId] = entry.KnowledgeTagId;
                }
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncSystems(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonSystemEntry>("systems", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} systems...");
        var existing = await _db.Systems.AsNoTracking().ToListAsync(ct);
        var existingEais = new HashSet<string>(existing.Select(s => s.EAI), StringComparer.OrdinalIgnoreCase);
        var dbByEai = existing.ToDictionary(s => s.EAI, StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            if (existingEais.Contains(entry.EAI))
            {
                var dbSys = dbByEai[entry.EAI];
                var action = ResolveConflict("System", entry.EAI, entry, dbSys);
                if (action is ConflictAction.Skip or ConflictAction.AlwaysSkip)
                {
                    _systemIdMap[entry.SystemId] = dbSys.SystemId;
                    continue;
                }
                if (action is ConflictAction.Update or ConflictAction.AlwaysUpdate)
                {
                    if (!_dryRun)
                    {
                        var sys = await _db.Systems.FindAsync([dbSys.SystemId], ct);
                        sys?.Update(entry.EAI, entry.Name, entry.OwnerUserId);
                    }
                    _systemIdMap[entry.SystemId] = dbSys.SystemId;
                }
            }
            else
            {
                if (!_dryRun)
                {
                    var sys = new AxiomSystem(entry.EAI, entry.Name, entry.OwnerUserId);
                    _db.Systems.Add(sys);
                    await _db.SaveChangesAsync(ct);
                    _systemIdMap[entry.SystemId] = sys.SystemId;
                }
                else
                {
                    _systemIdMap[entry.SystemId] = entry.SystemId;
                }
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncTechnicalComponents(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonTechnicalComponentEntry>("components", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} technical components...");
        var existing = await _db.TechnicalComponents.AsNoTracking().ToListAsync(ct);
        var existingIds = new HashSet<Guid>(existing.Select(c => c.ComponentId));

        foreach (var entry in entries)
        {
            var remappedSystemId = _systemIdMap.GetValueOrDefault(entry.SystemId, entry.SystemId);

            if (existingIds.Contains(entry.ComponentId))
            {
                var dbComp = existing.First(c => c.ComponentId == entry.ComponentId);
                var action = ResolveConflict("TechnicalComponent", entry.ComponentId.ToString(), entry, dbComp);
                if (action is ConflictAction.Skip or ConflictAction.AlwaysSkip)
                {
                    _componentIdMap[entry.ComponentId] = dbComp.ComponentId;
                    continue;
                }
                if (action is ConflictAction.Update or ConflictAction.AlwaysUpdate)
                {
                    if (!_dryRun)
                    {
                        var comp = await _db.TechnicalComponents.FindAsync([entry.ComponentId], ct);
                        comp?.Update(entry.Name, entry.TechnicalName,
                            Enum.Parse<Domain.Enums.ComponentType>(entry.ComponentType, true),
                            Enum.Parse<Domain.Enums.TargetEnvironment>(entry.Environment, true),
                            Enum.Parse<Domain.Enums.Criticality>(entry.Criticality, true),
                            remappedSystemId,
                            entry.Description);
                    }
                    _componentIdMap[entry.ComponentId] = dbComp.ComponentId;
                }
            }
            else
            {
                if (!_dryRun)
                {
                    var comp = new TechnicalComponent(
                        entry.Name,
                        entry.TechnicalName,
                        Enum.Parse<Domain.Enums.ComponentType>(entry.ComponentType, true),
                        Enum.Parse<Domain.Enums.TargetEnvironment>(entry.Environment, true),
                        Enum.Parse<Domain.Enums.Criticality>(entry.Criticality, true),
                        remappedSystemId,
                        entry.Description);
                    _db.TechnicalComponents.Add(comp);
                }
                _componentIdMap[entry.ComponentId] = entry.ComponentId;
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncIssues(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonIssueEntry>("issues", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} issues...");
        var existing = await _db.Issues.AsNoTracking().ToListAsync(ct);
        var existingIds = new HashSet<Guid>(existing.Select(i => i.IssueId));

        foreach (var entry in entries)
        {
            if (existingIds.Contains(entry.IssueId))
            {
                var dbIssue = existing.First(i => i.IssueId == entry.IssueId);
                var action = ResolveConflict("Issue", entry.IssueId.ToString(), entry, dbIssue);
                if (action is ConflictAction.Skip or ConflictAction.AlwaysSkip)
                {
                    _issueIdMap[entry.IssueId] = dbIssue.IssueId;
                    continue;
                }
                if (action is ConflictAction.Update or ConflictAction.AlwaysUpdate)
                {
                    if (!_dryRun)
                    {
                        var issue = await _db.Issues.FindAsync([entry.IssueId], ct);
                        var remappedSystemId = _systemIdMap.GetValueOrDefault(entry.SystemId, entry.SystemId);
                        issue?.Update(
                            entry.Summary,
                            entry.Problem,
                            entry.Analysis,
                            entry.Resolution,
                            remappedSystemId,
                            entry.StateId,
                            entry.RitmNumber,
                            entry.IncidentNumber);
                    }
                    _issueIdMap[entry.IssueId] = dbIssue.IssueId;
                }
            }
            else
            {
                if (!_dryRun)
                {
                    var remappedSystemId = _systemIdMap.GetValueOrDefault(entry.SystemId, entry.SystemId);
                    var issue = new Issue(
                        entry.Summary,
                        remappedSystemId,
                        entry.Problem,
                        entry.StateId,
                        entry.CreatedByUserId,
                        entry.Analysis,
                        entry.Resolution,
                        entry.RitmNumber,
                        entry.IncidentNumber,
                        entry.IssueId);
                    _db.Issues.Add(issue);
                }
                _issueIdMap[entry.IssueId] = entry.IssueId;
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncComponentDependencies(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonComponentDependencyEntry>("dependencies", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} component dependencies...");
        var existing = await _db.ComponentDependencies.AsNoTracking().ToListAsync(ct);
        var existingIds = new HashSet<Guid>(existing.Select(d => d.DependencyId));

        foreach (var entry in entries)
        {
            if (existingIds.Contains(entry.DependencyId))
            {
                _dependencyIdMap[entry.DependencyId] = entry.DependencyId;
            }
            else
            {
                if (!_dryRun)
                {
                    var remappedSource = _componentIdMap.GetValueOrDefault(entry.SourceComponentId, entry.SourceComponentId);
                    var remappedTarget = _componentIdMap.GetValueOrDefault(entry.TargetComponentId, entry.TargetComponentId);
                    var dep = new ComponentDependency(
                        remappedSource,
                        remappedTarget,
                        Enum.Parse<Domain.Enums.DependencyType>(entry.DependencyType, true),
                        Enum.Parse<Domain.Enums.Criticality>(entry.Criticality, true),
                        Enum.Parse<Domain.Enums.DependencyStatus>(entry.Status, true),
                        entry.Description,
                        entry.DependencyId);
                    _db.ComponentDependencies.Add(dep);
                }
                _dependencyIdMap[entry.DependencyId] = entry.DependencyId;
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncDependencyTraceEvents(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonDependencyTraceEventEntry>("trace-events", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} trace events...");
        var existing = await _db.DependencyTraceEvents.AsNoTracking().ToListAsync(ct);
        var existingIds = new HashSet<Guid>(existing.Select(e => e.TraceEventId));

        foreach (var entry in entries)
        {
            if (existingIds.Contains(entry.TraceEventId)) continue;

            if (!_dryRun)
            {
                var remappedDependencyId = _dependencyIdMap.GetValueOrDefault(entry.DependencyId, entry.DependencyId);
                var traceEvent = new DependencyTraceEvent(
                    remappedDependencyId,
                    Enum.Parse<Domain.Enums.DependencyTraceEventType>(entry.EventType, true),
                    entry.Description,
                    entry.IssueId,
                    entry.KnowledgeId,
                    entry.RitmNumber,
                    entry.ChangeNumber,
                    entry.CreatedByUserId,
                    entry.TraceEventId);
                _db.DependencyTraceEvents.Add(traceEvent);
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task SyncKnowledges(CancellationToken ct)
    {
        var entries = await _store.ReadAllAsync<JsonKnowledgeEntry>("knowledge", ct);
        if (entries.Count == 0) return;

        Console.WriteLine($"Syncing {entries.Count} knowledge entries...");
        var existing = await _db.Knowledges.AsNoTracking().ToListAsync(ct);
        var existingIds = new HashSet<Guid>(existing.Select(k => k.KnowledgeId));

        foreach (var entry in entries)
        {
            if (existingIds.Contains(entry.KnowledgeId)) continue;

            if (!_dryRun)
            {
                var remappedSystemId = _systemIdMap.GetValueOrDefault(entry.SystemId, entry.SystemId);
                var remappedTypeId = _typeIdMap.GetValueOrDefault(entry.KnowledgeTypeId, entry.KnowledgeTypeId);
                var remappedKsId = _ksIdMap.GetValueOrDefault(entry.KnowledgeStateId, entry.KnowledgeStateId);
                var remappedIssueId = entry.IssueId.HasValue
                    ? _issueIdMap.GetValueOrDefault(entry.IssueId.Value, entry.IssueId.Value)
                    : (Guid?)null;

                var knowledge = new Knowledge(
                    entry.Title,
                    entry.Summary,
                    entry.Content,
                    remappedSystemId,
                    entry.CreatedByUserId,
                    remappedTypeId,
                    remappedKsId,
                    remappedIssueId,
                    entry.KnowledgeId);

                if (entry.Tags.Count > 0)
                {
                    var allTags = await _db.KnowledgeTags.AsNoTracking().ToListAsync(ct);
                    var tagMap = allTags.ToDictionary(t => t.TagName, t => t.KnowledgeTagId, StringComparer.OrdinalIgnoreCase);

                    foreach (var tagName in entry.Tags)
                    {
                        if (tagMap.TryGetValue(tagName, out var tagId))
                        {
                            knowledge.KnowledgeKnowledgeTags.Add(new KnowledgeKnowledgeTag(entry.KnowledgeId, tagId));
                        }
                    }
                }

                _db.Knowledges.Add(knowledge);
            }
        }

        if (!_dryRun) await _db.SaveChangesAsync(ct);
    }

    private async Task ArchiveSyncedFiles(CancellationToken ct)
    {
        var basePath = _store.BasePath;
        var archiveDir = Path.Combine(basePath, "archives", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        Directory.CreateDirectory(archiveDir);

        var dataDir = new DirectoryInfo(basePath);
        foreach (var file in dataDir.GetFiles("*.json"))
        {
            var dest = Path.Combine(archiveDir, file.Name);
            file.MoveTo(dest);
            Console.WriteLine($"  Archived: {file.Name}");
        }
    }

    private ConflictAction ResolveConflict(string entityType, string identifier, object local, object db)
    {
        var info = new SyncConflictInfo
        {
            EntityType = entityType,
            Identifier = identifier,
            LocalEntry = local,
            DbEntry = db
        };

        return _conflictResolver?.Invoke(info) ?? ConflictAction.Skip;
    }
}
