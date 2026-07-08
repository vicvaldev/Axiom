namespace Axiom.Application.Interfaces;

public enum ConflictAction
{
    Skip,
    Update,
    AlwaysSkip,
    AlwaysUpdate
}

public class SyncConflictInfo
{
    public required string EntityType { get; set; }
    public required string Identifier { get; set; }
    public required object LocalEntry { get; set; }
    public required object DbEntry { get; set; }
}

public interface ISyncService
{
    Task<bool> SyncAsync(bool dryRun, CancellationToken ct);
}
