namespace Axiom.Api.Dtos.Requests;

public record CreateComponentRequest(
    string Name,
    string TechnicalName,
    string ComponentType,
    string Environment,
    string Criticality,
    long SystemId,
    string? Description);
