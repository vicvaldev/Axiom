using Axiom.Api.Dtos.Requests;
using Axiom.Application.Commands;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Api.ApiEndpoints;

public static class IssueEndpoints
{
    public static RouteGroupBuilder MapIssues(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateIssueRequest req, IMediator mediator) =>
        {
            var cmd = new CreateIssueCommand(
                req.Summary, req.SystemId, req.Problem,
                req.Analysis ?? string.Empty, req.Resolution ?? string.Empty,
                req.StateId, req.CreatedByUserId, req.RitmNumber, req.IncidentNumber, Guid.NewGuid());
            var result = await mediator.Send(cmd);
            return Results.Created($"/api/issues/{result.IssueId}", result);
        });

        group.MapPut("/{id}", async (Guid id, UpdateIssueRequest req, IMediator mediator) =>
        {
            var cmd = new UpdateIssueCommand(
                id, req.Summary, req.Problem, req.Analysis, req.Resolution,
                req.SystemId, req.StateId, req.RitmNumber, req.IncidentNumber);
            var result = await mediator.Send(cmd);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapGet("/", async (string? eai, IMediator mediator) =>
            Results.Ok(await mediator.Send(new ListIssuesQuery(eai))));

        group.MapGet("/{id}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetIssueByIdQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapDelete("/{id}", async (Guid id, IMediator mediator) =>
        {
            var ok = await mediator.Send(new DeleteIssueCommand(id));
            return ok ? Results.Ok(new { deleted = true, id }) : Results.NotFound();
        });

        return group;
    }
}
