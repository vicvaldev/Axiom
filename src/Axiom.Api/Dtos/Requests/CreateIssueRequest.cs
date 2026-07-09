namespace Axiom.Api.Dtos.Requests;

public record CreateIssueRequest(
    string Summary,
    long SystemId,
    string Problem,
    string? Analysis,
    string? Resolution,
    int StateId,
    Guid CreatedByUserId,
    string? RitmNumber,
    string? IncidentNumber);
