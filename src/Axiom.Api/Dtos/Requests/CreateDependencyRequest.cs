namespace Axiom.Api.Dtos.Requests;

public record CreateDependencyRequest(
    Guid SourceComponentId,
    Guid TargetComponentId,
    string DependencyType,
    string Criticality,
    string Status,
    string? Description);
