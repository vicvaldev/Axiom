using Axiom.Api.Dtos.Requests;
using Axiom.Application.Commands;
using Axiom.Application.Queries;
using Axiom.Domain.Enums;
using MediatR;

namespace Axiom.Api.ApiEndpoints;

public static class DependencyEndpoints
{
    public static RouteGroupBuilder MapDependencies(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateDependencyRequest req, IMediator mediator) =>
        {
            if (!Enum.TryParse<DependencyType>(req.DependencyType, true, out var depType))
                return Results.BadRequest($"Invalid dependency type. Valid values: {string.Join(", ", Enum.GetNames<DependencyType>())}");

            if (!Enum.TryParse<Criticality>(req.Criticality, true, out var criticality))
                return Results.BadRequest($"Invalid criticality. Valid values: {string.Join(", ", Enum.GetNames<Criticality>())}");

            if (!Enum.TryParse<DependencyStatus>(req.Status, true, out var status))
                return Results.BadRequest($"Invalid status. Valid values: {string.Join(", ", Enum.GetNames<DependencyStatus>())}");

            var cmd = new CreateComponentDependencyCommand(
                req.SourceComponentId, req.TargetComponentId, depType, criticality, status, req.Description);
            var result = await mediator.Send(cmd);
            return Results.Created($"/api/dependencies/{result.DependencyId}", result);
        });

        group.MapGet("/", async (Guid componentId, IMediator mediator) =>
            Results.Ok(await mediator.Send(new ListDependenciesByComponentQuery(componentId))));

        group.MapGet("/{id}/impact", async (Guid id, IMediator mediator) =>
            Results.Ok(await mediator.Send(new ListImpactedComponentsQuery(id))));

        group.MapPost("/trace", async (CreateTraceEventRequest req, IMediator mediator) =>
        {
            if (!Enum.TryParse<DependencyTraceEventType>(req.EventType, true, out var eventType))
                return Results.BadRequest($"Invalid event type. Valid values: {string.Join(", ", Enum.GetNames<DependencyTraceEventType>())}");

            var cmd = new CreateDependencyTraceEventCommand(
                req.DependencyId, eventType, req.Description,
                req.IssueId, req.KnowledgeId, req.RitmNumber, req.ChangeNumber, req.CreatedByUserId);
            var result = await mediator.Send(cmd);
            return Results.Created($"/api/dependencies/trace/{result.TraceEventId}", result);
        });

        return group;
    }
}
