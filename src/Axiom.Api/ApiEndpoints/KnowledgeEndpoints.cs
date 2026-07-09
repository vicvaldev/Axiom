using Axiom.Api.Dtos.Requests;
using Axiom.Application.Commands;
using Axiom.Application.Queries;
using MediatR;

namespace Axiom.Api.ApiEndpoints;

public static class KnowledgeEndpoints
{
    public static RouteGroupBuilder MapKnowledge(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateKnowledgeRequest req, IMediator mediator) =>
        {
            var cmd = new CreateKnowledgeCommand(
                req.Title, req.Summary, req.Content, req.SystemId,
                req.CreatedByUserId, req.KnowledgeTypeId, req.KnowledgeStateId,
                req.IssueId, req.Tags, Guid.NewGuid());
            var result = await mediator.Send(cmd);
            return Results.Created($"/api/knowledge/{result.KnowledgeId}", result);
        });

        group.MapPut("/{id}", async (Guid id, UpdateKnowledgeRequest req, IMediator mediator) =>
        {
            var cmd = new UpdateKnowledgeCommand(
                id, req.Title, req.Summary, req.Content, req.SystemId,
                req.KnowledgeTypeId, req.KnowledgeStateId, req.IssueId, req.Tags);
            var result = await mediator.Send(cmd);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapGet("/", async (IMediator mediator) =>
            Results.Ok(await mediator.Send(new ListKnowledgeQuery())));

        group.MapGet("/{id}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetKnowledgeByIdQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapGet("/search", async (string q, IMediator mediator) =>
            Results.Ok(await mediator.Send(new SearchKnowledgeQuery(q))));

        group.MapPost("/ask", async (string question, IMediator mediator) =>
            Results.Ok(await mediator.Send(new AskQuery(question))));

        return group;
    }
}
