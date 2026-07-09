namespace Axiom.Api.Dtos.Requests;

public record UpdateIssueRequest(
    string Summary,
    long SystemId,
    string Problem,
    string? Analysis,
    string? Resolution,
    int StateId,
    string? RitmNumber,
    string? IncidentNumber);
