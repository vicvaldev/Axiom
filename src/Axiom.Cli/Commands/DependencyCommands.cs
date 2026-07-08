using System.CommandLine;
using System.CommandLine.Parsing;
using Axiom.Application.Commands;
using Axiom.Application.Dtos;
using Axiom.Application.Queries;
using Axiom.Cli.Helpers;
using Axiom.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Axiom.Cli.Commands;

internal static class DependencyCommands
{
    public static Command Create(IHost host)
    {
        var cmd = new Command("dependency", "Manage component dependencies");

        cmd.Subcommands.Add(CreateAdd(host));
        cmd.Subcommands.Add(CreateList(host));
        cmd.Subcommands.Add(CreateImpact(host));
        cmd.Subcommands.Add(CreateTrace(host));

        return cmd;
    }

    private static Command CreateAdd(IHost host)
    {
        var addCmd = new Command("add", "Add a new dependency between components");
        var sourceOpt = new Option<Guid>("--source") { Required = true };
        var targetOpt = new Option<Guid>("--target") { Required = true };
        var typeOpt = new Option<string>("--type") { Required = true };
        var criticalityOpt = new Option<string>("--criticality") { Required = true };
        var statusOpt = new Option<string>("--status") { Required = true };
        var descriptionOpt = new Option<string>("--description");
        var jsonOpt = CliOutput.NewJsonOption();
        addCmd.Options.Add(sourceOpt);
        addCmd.Options.Add(targetOpt);
        addCmd.Options.Add(typeOpt);
        addCmd.Options.Add(criticalityOpt);
        addCmd.Options.Add(statusOpt);
        addCmd.Options.Add(descriptionOpt);
        addCmd.Options.Add(jsonOpt);

        addCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(jsonOpt);

            if (!Enum.TryParse<DependencyType>(result.GetValue(typeOpt), true, out var depType))
            {
                CliOutput.WriteError($"Invalid dependency type. Valid values: {string.Join(", ", Enum.GetNames<DependencyType>())}", json);
                return;
            }

            if (!Enum.TryParse<Criticality>(result.GetValue(criticalityOpt), true, out var criticality))
            {
                CliOutput.WriteError($"Invalid criticality. Valid values: {string.Join(", ", Enum.GetNames<Criticality>())}", json);
                return;
            }

            if (!Enum.TryParse<DependencyStatus>(result.GetValue(statusOpt), true, out var status))
            {
                CliOutput.WriteError($"Invalid status. Valid values: {string.Join(", ", Enum.GetNames<DependencyStatus>())}", json);
                return;
            }

            var command = new CreateComponentDependencyCommand(
                result.GetValue(sourceOpt),
                result.GetValue(targetOpt),
                depType,
                criticality,
                status,
                result.GetValue(descriptionOpt));

            try
            {
                var entry = mediator.Send(command).Result;

                if (json)
                {
                    CliOutput.WriteJson(Mappers.ToDependencyCreateResult(entry));
                    return;
                }

                AnsiConsole.MarkupLine($"[green]Dependency created:[/] {entry.DependencyId}");
                AnsiConsole.MarkupLine($"  [bold]Source:[/] {entry.SourceComponentId}");
                AnsiConsole.MarkupLine($"  [bold]Target:[/] {entry.TargetComponentId}");
                AnsiConsole.MarkupLine($"  [bold]Type:[/] {entry.DependencyType}");
                AnsiConsole.MarkupLine($"  [bold]Status:[/] {entry.Status}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                var entry = new JsonComponentDependencyEntry
                {
                    DependencyId = Guid.NewGuid(),
                    SourceComponentId = result.GetValue(sourceOpt),
                    TargetComponentId = result.GetValue(targetOpt),
                    DependencyType = result.GetValue(typeOpt)!,
                    Criticality = result.GetValue(criticalityOpt)!,
                    Status = result.GetValue(statusOpt)!,
                    Description = result.GetValue(descriptionOpt)
                };

                store.AppendAsync("dependencies", entry).Wait();
                CliOutput.WriteError("Database is not available. Data saved locally.", json);
            }
        });

        return addCmd;
    }

    private static Command CreateList(IHost host)
    {
        var listCmd = new Command("list", "List dependencies of a component");
        var componentOpt = new Option<Guid>("--component") { Required = true };
        var jsonOpt = CliOutput.NewJsonOption();
        listCmd.Options.Add(componentOpt);
        listCmd.Options.Add(jsonOpt);

        listCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(jsonOpt);

            try
            {
                var entries = mediator.Send(new ListDependenciesByComponentQuery(result.GetValue(componentOpt))).Result;

                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                var table = new Table();
                table.AddColumns("Id", "Source", "Target", "Type", "Status", "Criticality");

                foreach (var entry in entries)
                {
                    table.AddRow(
                        entry.DependencyId.ToString()[..8],
                        entry.SourceTechnicalName,
                        entry.TargetTechnicalName,
                        entry.DependencyType,
                        entry.Status,
                        entry.Criticality);
                }

                AnsiConsole.Write(table);
            }
            catch
            {
                CliOutput.WriteError("Failed to list dependencies. Database may be unavailable.", json);
            }
        });

        return listCmd;
    }

    private static Command CreateImpact(IHost host)
    {
        var impactCmd = new Command("impact", "List components impacted by a component");
        var componentOpt = new Option<Guid>("--component") { Required = true };
        var jsonOpt = CliOutput.NewJsonOption();
        impactCmd.Options.Add(componentOpt);
        impactCmd.Options.Add(jsonOpt);

        impactCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(jsonOpt);

            try
            {
                var entries = mediator.Send(new ListImpactedComponentsQuery(result.GetValue(componentOpt))).Result;

                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                var table = new Table();
                table.AddColumns("Id", "Source", "Target", "Type", "Status", "Criticality");

                foreach (var entry in entries)
                {
                    table.AddRow(
                        entry.DependencyId.ToString()[..8],
                        entry.SourceTechnicalName,
                        entry.TargetTechnicalName,
                        entry.DependencyType,
                        entry.Status,
                        entry.Criticality);
                }

                AnsiConsole.Write(table);
            }
            catch
            {
                CliOutput.WriteError("Failed to get impact data. Database may be unavailable.", json);
            }
        });

        return impactCmd;
    }

    private static Command CreateTrace(IHost host)
    {
        var traceCmd = new Command("trace", "Register a trace event on a dependency");
        var depOpt = new Option<Guid>("--dependency") { Required = true };
        var eventTypeOpt = new Option<string>("--event-type") { Required = true };
        var descriptionOpt = new Option<string>("--description") { Required = true };
        var issueIdOpt = new Option<Guid?>("--issue-id");
        var knowledgeIdOpt = new Option<Guid?>("--knowledge-id");
        var ritmOpt = new Option<string>("--ritm-number");
        var changeOpt = new Option<string>("--change-number");
        var userIdOpt = new Option<Guid?>("--created-by");
        var jsonOpt = CliOutput.NewJsonOption();
        traceCmd.Options.Add(depOpt);
        traceCmd.Options.Add(eventTypeOpt);
        traceCmd.Options.Add(descriptionOpt);
        traceCmd.Options.Add(issueIdOpt);
        traceCmd.Options.Add(knowledgeIdOpt);
        traceCmd.Options.Add(ritmOpt);
        traceCmd.Options.Add(changeOpt);
        traceCmd.Options.Add(userIdOpt);
        traceCmd.Options.Add(jsonOpt);

        traceCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(jsonOpt);

            if (!Enum.TryParse<DependencyTraceEventType>(result.GetValue(eventTypeOpt), true, out var eventType))
            {
                CliOutput.WriteError($"Invalid event type. Valid values: {string.Join(", ", Enum.GetNames<DependencyTraceEventType>())}", json);
                return;
            }

            var command = new CreateDependencyTraceEventCommand(
                result.GetValue(depOpt),
                eventType,
                result.GetValue(descriptionOpt)!,
                result.GetValue(issueIdOpt),
                result.GetValue(knowledgeIdOpt),
                result.GetValue(ritmOpt),
                result.GetValue(changeOpt),
                result.GetValue(userIdOpt));

            try
            {
                var entry = mediator.Send(command).Result;

                if (json)
                {
                    CliOutput.WriteJson(Mappers.ToTraceEventResult(entry));
                    return;
                }

                AnsiConsole.MarkupLine($"[green]Trace event registered:[/] {entry.TraceEventId}");
                AnsiConsole.MarkupLine($"  [bold]Event Type:[/] {entry.EventType}");
                AnsiConsole.MarkupLine($"  [bold]Description:[/] {entry.Description}");
                AnsiConsole.MarkupLine($"  [bold]Dependency:[/] {entry.DependencyId}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                var entry = new JsonDependencyTraceEventEntry
                {
                    TraceEventId = Guid.NewGuid(),
                    DependencyId = result.GetValue(depOpt),
                    EventType = result.GetValue(eventTypeOpt)!,
                    Description = result.GetValue(descriptionOpt)!,
                    IssueId = result.GetValue(issueIdOpt),
                    KnowledgeId = result.GetValue(knowledgeIdOpt),
                    RitmNumber = result.GetValue(ritmOpt),
                    ChangeNumber = result.GetValue(changeOpt),
                    CreatedByUserId = result.GetValue(userIdOpt),
                    CreatedAt = DateTime.UtcNow
                };

                store.AppendAsync("trace-events", entry).Wait();
                CliOutput.WriteError("Database is not available. Data saved locally.", json);
            }
        });

        return traceCmd;
    }
}
