using System.CommandLine;
using System.CommandLine.Parsing;
using Axiom.Application.Commands;
using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Cli.Helpers;
using Axiom.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Axiom.Cli.Commands;

internal static class ComponentCommands
{
    public static Command Create(IHost host)
    {
        var cmd = new Command("component", "Manage technical components");

        cmd.Subcommands.Add(CreateCreate(host));
        cmd.Subcommands.Add(CreateList(host));
        cmd.Subcommands.Add(CreateShow(host));

        return cmd;
    }

    private static Command CreateCreate(IHost host)
    {
        var createCmd = new Command("add", "Add a new technical component");
        var nameOpt = new Option<string>("--name") { Required = true };
        var technicalNameOpt = new Option<string>("--technical-name") { Required = true };
        var componentTypeOpt = new Option<string>("--type") { Required = true };
        var envOpt = new Option<string>("--environment") { Required = true };
        var criticalityOpt = new Option<string>("--criticality") { Required = true };
        var systemIdOpt = new Option<long>("--system-id") { Required = true };
        var descriptionOpt = new Option<string>("--description");
        var jsonOpt = CliOutput.NewJsonOption();
        createCmd.Options.Add(nameOpt);
        createCmd.Options.Add(technicalNameOpt);
        createCmd.Options.Add(componentTypeOpt);
        createCmd.Options.Add(envOpt);
        createCmd.Options.Add(criticalityOpt);
        createCmd.Options.Add(systemIdOpt);
        createCmd.Options.Add(descriptionOpt);
        createCmd.Options.Add(jsonOpt);

        createCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(jsonOpt);

            if (!Enum.TryParse<ComponentType>(result.GetValue(componentTypeOpt), true, out var componentType))
            {
                CliOutput.WriteError($"Invalid component type. Valid values: {string.Join(", ", Enum.GetNames<ComponentType>())}", json);
                return;
            }

            if (!Enum.TryParse<TargetEnvironment>(result.GetValue(envOpt), true, out var env))
            {
                CliOutput.WriteError($"Invalid environment. Valid values: {string.Join(", ", Enum.GetNames<TargetEnvironment>())}", json);
                return;
            }

            if (!Enum.TryParse<Criticality>(result.GetValue(criticalityOpt), true, out var criticality))
            {
                CliOutput.WriteError($"Invalid criticality. Valid values: {string.Join(", ", Enum.GetNames<Criticality>())}", json);
                return;
            }

            var command = new CreateTechnicalComponentCommand(
                result.GetValue(nameOpt)!,
                result.GetValue(technicalNameOpt)!,
                componentType,
                env,
                criticality,
                result.GetValue(systemIdOpt),
                result.GetValue(descriptionOpt));

            try
            {
                var entry = mediator.Send(command).Result;

                if (json)
                {
                    CliOutput.WriteJson(Mappers.ToComponentCreateResult(entry));
                    return;
                }

                AnsiConsole.MarkupLine($"[green]Component created:[/] {entry.ComponentId}");
                AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
                AnsiConsole.MarkupLine($"  [bold]Technical Name:[/] {entry.TechnicalName}");
                AnsiConsole.MarkupLine($"  [bold]Type:[/] {entry.ComponentType}");
                AnsiConsole.MarkupLine($"  [bold]System ID:[/] {entry.SystemId}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                var entry = new JsonTechnicalComponentEntry
                {
                    ComponentId = Guid.NewGuid(),
                    Name = result.GetValue(nameOpt)!,
                    TechnicalName = result.GetValue(technicalNameOpt)!,
                    ComponentType = result.GetValue(componentTypeOpt)!,
                    Environment = result.GetValue(envOpt)!,
                    Criticality = result.GetValue(criticalityOpt)!,
                    Description = result.GetValue(descriptionOpt),
                    SystemId = result.GetValue(systemIdOpt),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                store.AppendAsync("components", entry).Wait();
                CliOutput.WriteError("Database is not available. Data saved locally.", json);
            }
        });

        return createCmd;
    }

    private static Command CreateList(IHost host)
    {
        var listCmd = new Command("list", "List technical components");
        var systemIdOpt = new Option<long>("--system-id") { Required = true };
        var jsonOpt = CliOutput.NewJsonOption();
        listCmd.Options.Add(systemIdOpt);
        listCmd.Options.Add(jsonOpt);

        listCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(jsonOpt);

            try
            {
                var entries = mediator.Send(new ListComponentsBySystemQuery(result.GetValue(systemIdOpt))).Result;

                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                var table = new Table();
                table.AddColumns("Id", "Name", "Technical Name", "Type", "Environment", "Criticality");

                foreach (var entry in entries)
                {
                    table.AddRow(
                        entry.ComponentId.ToString()[..8],
                        entry.Name,
                        entry.TechnicalName,
                        entry.ComponentType,
                        entry.Environment,
                        entry.Criticality);
                }

                AnsiConsole.Write(table);
            }
            catch
            {
                CliOutput.WriteError("Failed to list components. Database may be unavailable.", json);
            }
        });

        return listCmd;
    }

    private static Command CreateShow(IHost host)
    {
        var showCmd = new Command("show", "Show component details");
        var idArg = new Argument<Guid>("id");
        var jsonOpt = CliOutput.NewJsonOption();
        showCmd.Arguments.Add(idArg);
        showCmd.Options.Add(jsonOpt);

        showCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var id = result.GetValue(idArg);
            var json = result.GetValue(jsonOpt);

            try
            {
                var entry = mediator.Send(new GetComponentByIdQuery(id)).Result;

                if (entry is null)
                {
                    CliOutput.WriteError("Component not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(entry);
                    return;
                }

                var panel = new Panel(
                    new Markup(
                        $"[bold]Name:[/] {entry.Name}\n" +
                        $"[bold]Technical Name:[/] {entry.TechnicalName}\n" +
                        $"[bold]Type:[/] {entry.ComponentType}\n" +
                        $"[bold]Environment:[/] {entry.Environment}\n" +
                        $"[bold]Criticality:[/] {entry.Criticality}\n" +
                        $"[bold]Description:[/] {entry.Description ?? "-"}\n" +
                        $"[bold]System:[/] {entry.SystemName}\n" +
                        $"[bold]Created:[/] {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                        $"[bold]Updated:[/] {entry.UpdatedAt:yyyy-MM-dd HH:mm:ss}"))
                {
                    Header = new PanelHeader($"Component - {entry.ComponentId}")
                };

                AnsiConsole.Write(panel);
            }
            catch
            {
                CliOutput.WriteError("Failed to show component. Database may be unavailable.", json);
            }
        });

        return showCmd;
    }
}
