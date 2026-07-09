using Axiom.Application.Commands;
using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using MediatR;

namespace Axiom.Api.ApiEndpoints;

public static class ReferenceEndpoints
{
    public static RouteGroupBuilder MapReference(this RouteGroupBuilder group)
    {
        var knowledgeTypes = group.MapGroup("/knowledge-types");
        MapCrud(knowledgeTypes,
            list: refs => refs.ListKnowledgeTypesAsync(),
            create: (code, name) => new CreateKnowledgeTypeCommand(code, name),
            update: (id, code, name) => new UpdateKnowledgeTypeCommand(id, code, name),
            del: (id) => new DeleteKnowledgeTypeCommand((long)id));

        var knowledgeStates = group.MapGroup("/knowledge-states");
        MapCrud(knowledgeStates,
            list: refs => refs.ListKnowledgeStatesAsync(),
            create: (code, name) => new CreateKnowledgeStateCommand(code, name),
            update: (id, code, name) => new UpdateKnowledgeStateCommand((int)id, code, name),
            del: (id) => new DeleteKnowledgeStateCommand((int)id));

        var issueStates = group.MapGroup("/issue-states");
        MapCrud(issueStates,
            list: refs => refs.ListIssueStatesAsync(),
            create: (code, name) => new CreateIssueStateCommand(code, name),
            update: (id, code, name) => new UpdateIssueStateCommand((int)id, code, name),
            del: (id) => new DeleteIssueStateCommand((int)id));

        group.MapGroup("/knowledge-tags")
            .MapTagCrud();

        return group;
    }

    private static void MapCrud(
        RouteGroupBuilder group,
        Func<IReferenceDataService, Task<IReadOnlyList<ReferenceCodeDto>>> list,
        Func<string, string, object> create,
        Func<long, string, string, object> update,
        Func<long, object> del)
    {
        group.MapGet("/", async (IReferenceDataService refs) =>
            Results.Ok(await list(refs)));

        group.MapPost("/", async (ReferenceCodeDto req, IMediator mediator) =>
        {
            var cmd = create(req.Code, req.Name);
            var result = await mediator.Send(cmd);
            return Results.Created(string.Empty, result);
        });

        group.MapPut("/{id:long}", async (long id, ReferenceCodeDto req, IMediator mediator) =>
        {
            var cmd = update(id, req.Code, req.Name);
            var result = await mediator.Send(cmd);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapDelete("/{id:long}", async (long id, IMediator mediator) =>
        {
            var ok = await mediator.Send(del(id));
            return ok is true ? Results.Ok(new { deleted = true, id }) : Results.NotFound();
        });
    }

    private static void MapTagCrud(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IKnowledgeTagRepository repo) =>
        {
            var tags = await repo.GetAllAsync();
            var items = tags.Select(t => new ReferenceCodeDto { Id = t.KnowledgeTagId, Code = t.TagName, Name = t.TagName });
            return Results.Ok(items);
        });

        group.MapPost("/", async (ReferenceCodeDto req, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateKnowledgeTagCommand(req.Name));
            return Results.Created(string.Empty, result);
        });

        group.MapPut("/{id:long}", async (long id, ReferenceCodeDto req, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateKnowledgeTagCommand(id, req.Name));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapDelete("/{id:long}", async (long id, IMediator mediator) =>
        {
            var ok = await mediator.Send(new DeleteKnowledgeTagCommand(id));
            return ok ? Results.Ok(new { deleted = true, id }) : Results.NotFound();
        });
    }
}
