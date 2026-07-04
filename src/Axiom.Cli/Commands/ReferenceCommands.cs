using System.CommandLine;
using System.CommandLine.Parsing;
using Axiom.Application.Commands;
using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Cli.Helpers;
using Axiom.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Axiom.Cli.Commands;

internal static class ReferenceCommands
{
    public static Command Create(IHost host)
    {
        var cmd = new Command("reference", "Manage reference data");
        cmd.Subcommands.Add(CreateKnowledgeType(host));
        cmd.Subcommands.Add(CreateKnowledgeState(host));
        cmd.Subcommands.Add(CreateIssueState(host));
        cmd.Subcommands.Add(CreateKnowledgeTag(host));
        return cmd;
    }

    private static Command CreateKnowledgeType(IHost host)
    {
        var cmd = new Command("knowledge-type", "Manage knowledge types");
        cmd.Subcommands.Add(CreateListCmd(host, "list", "List knowledge types",
            refs => refs.ListKnowledgeTypesAsync().Result));
        cmd.Subcommands.Add(CreateCreateCmd(host, "create", "Create a new knowledge type",
            (code, name) => new CreateKnowledgeTypeCommand(code, name)));
        cmd.Subcommands.Add(CreateUpdateCmd(host, "update", "Update a knowledge type",
            (long id, string code, string name) => new UpdateKnowledgeTypeCommand(id, code, name)));
        cmd.Subcommands.Add(CreateDeleteCmd(host, "delete", "Delete a knowledge type",
            (long id) => new DeleteKnowledgeTypeCommand(id)));
        return cmd;
    }

    private static Command CreateKnowledgeState(IHost host)
    {
        var cmd = new Command("knowledge-state", "Manage knowledge states");
        cmd.Subcommands.Add(CreateListCmd(host, "list", "List knowledge states",
            refs => refs.ListKnowledgeStatesAsync().Result));
        cmd.Subcommands.Add(CreateCreateCmd(host, "create", "Create a new knowledge state",
            (code, name) => new CreateKnowledgeStateCommand(code, name)));
        cmd.Subcommands.Add(CreateUpdateCmd(host, "update", "Update a knowledge state",
            (long id, string code, string name) => new UpdateKnowledgeStateCommand((int)id, code, name)));
        cmd.Subcommands.Add(CreateDeleteCmd(host, "delete", "Delete a knowledge state",
            (long id) => new DeleteKnowledgeStateCommand((int)id)));
        return cmd;
    }

    private static Command CreateIssueState(IHost host)
    {
        var cmd = new Command("issue-state", "Manage issue states");
        cmd.Subcommands.Add(CreateListCmd(host, "list", "List issue states",
            refs => refs.ListIssueStatesAsync().Result));
        cmd.Subcommands.Add(CreateCreateCmd(host, "create", "Create a new issue state",
            (code, name) => new CreateIssueStateCommand(code, name)));
        cmd.Subcommands.Add(CreateUpdateCmd(host, "update", "Update an issue state",
            (long id, string code, string name) => new UpdateIssueStateCommand((int)id, code, name)));
        cmd.Subcommands.Add(CreateDeleteCmd(host, "delete", "Delete an issue state",
            (long id) => new DeleteIssueStateCommand((int)id)));
        return cmd;
    }

    private static Command CreateKnowledgeTag(IHost host)
    {
        var cmd = new Command("knowledge-tag", "Manage knowledge tags");
        cmd.Subcommands.Add(CreateTagListCmd(host, "list", "List all knowledge tags"));
        cmd.Subcommands.Add(CreateCreateCmd(host, "create", "Create a new knowledge tag",
            (_, name) => new CreateKnowledgeTagCommand(name)));
        cmd.Subcommands.Add(CreateUpdateCmd(host, "update", "Update a knowledge tag",
            (long id, string _, string name) => new UpdateKnowledgeTagCommand(id, name)));
        cmd.Subcommands.Add(CreateDeleteCmd(host, "delete", "Delete a knowledge tag",
            (long id) => new DeleteKnowledgeTagCommand(id)));
        return cmd;
    }

    private static Command CreateListCmd(IHost host, string name, string desc,
        Func<IReferenceDataService, IReadOnlyList<ReferenceCodeDto>> fetcher)
    {
        var cmd = new Command(name, desc);
        var jsonOpt = CliOutput.NewJsonOption();
        cmd.Options.Add(jsonOpt);

        cmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
            var json = result.GetValue(jsonOpt);

            try
            {
                var items = fetcher(references);
                CliOutput.WriteReferenceList(items, json, "Id", "Code", "Name");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available.", json);
                    return;
                }

                CliOutput.WriteError("Database is not available.", json);
            }
        });

        return cmd;
    }

    private static Command CreateTagListCmd(IHost host, string name, string desc)
    {
        var cmd = new Command(name, desc);
        var jsonOpt = CliOutput.NewJsonOption();
        cmd.Options.Add(jsonOpt);

        cmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var tagRepo = scope.ServiceProvider.GetRequiredService<IKnowledgeTagRepository>();
            var json = result.GetValue(jsonOpt);

            try
            {
                var tags = tagRepo.GetAllAsync().Result;
                var items = tags.Select(t => new ReferenceCodeDto { Id = t.KnowledgeTagId, Code = t.TagName, Name = t.TagName }).ToList();
                CliOutput.WriteReferenceList(items, json, "Id", "Name");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available.", json);
                    return;
                }

                var entries = store.ReadAllAsync<JsonKnowledgeTagEntry>("knowledge-tags").Result;
                if (json)
                {
                    CliOutput.WriteJson(entries);
                    return;
                }

                AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
                var table = new Table();
                table.AddColumns("Id", "Name");
                foreach (var entry in entries)
                {
                    table.AddRow(entry.KnowledgeTagId.ToString(), entry.TagName);
                }
                AnsiConsole.Write(table);
            }
        });

        return cmd;
    }

    private static Command CreateCreateCmd(IHost host, string name, string desc,
        Func<string, string, object> createCommand)
    {
        var cmd = new Command(name, desc);
        var codeOpt = new Option<string>("--code");
        var nameOpt = new Option<string>("--name") { Required = true };
        var jsonOpt = CliOutput.NewJsonOption();
        cmd.Options.Add(codeOpt);
        cmd.Options.Add(nameOpt);
        cmd.Options.Add(jsonOpt);

        cmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(jsonOpt);

            var code = result.GetValue(codeOpt) ?? string.Empty;
            var createName = result.GetValue(nameOpt)!;

            try
            {
                var command = createCommand(code, createName);
                var entry = mediator.Send(command).Result;

                if (json)
                {
                    CliOutput.WriteJson(new { entry });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]{desc}: done[/]");
                if (!string.IsNullOrWhiteSpace(code))
                    AnsiConsole.MarkupLine($"  [bold]Code:[/] {code}");
                AnsiConsole.MarkupLine($"  [bold]Name:[/] {createName}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                CliOutput.WriteError("Database is not available.", json);
            }
        });

        return cmd;
    }

    private static Command CreateUpdateCmd(IHost host, string name, string desc,
        Func<long, string, string, object> createCommand)
    {
        var cmd = new Command(name, desc);
        var idArg = new Argument<long>("id");
        var codeOpt = new Option<string>("--code") { Required = true };
        var nameOpt = new Option<string>("--name") { Required = true };
        var jsonOpt = CliOutput.NewJsonOption();
        cmd.Arguments.Add(idArg);
        cmd.Options.Add(codeOpt);
        cmd.Options.Add(nameOpt);
        cmd.Options.Add(jsonOpt);

        cmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(jsonOpt);

            var id = result.GetValue(idArg);
            var code = result.GetValue(codeOpt)!;
            var updateName = result.GetValue(nameOpt)!;

            try
            {
                var command = createCommand(id, code, updateName);
                var entry = mediator.Send(command).Result;

                if (entry is null)
                {
                    CliOutput.WriteError($"{desc} not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(new { Id = id, Code = code, Name = updateName });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]{desc}:[/] {id}");
                AnsiConsole.MarkupLine($"  [bold]Code:[/] {code}");
                AnsiConsole.MarkupLine($"  [bold]Name:[/] {updateName}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                CliOutput.WriteError("Database is not available.", json);
            }
        });

        return cmd;
    }

    private static Command CreateDeleteCmd(IHost host, string name, string desc,
        Func<long, object> createCommand)
    {
        var cmd = new Command(name, desc);
        var idArg = new Argument<long>("id");
        var jsonOpt = CliOutput.NewJsonOption();
        cmd.Arguments.Add(idArg);
        cmd.Options.Add(jsonOpt);

        cmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var json = result.GetValue(jsonOpt);

            var id = result.GetValue(idArg);

            try
            {
                var command = createCommand(id);
                var ok = mediator.Send(command).Result;
                if (ok is null or false)
                {
                    CliOutput.WriteError($"{desc} not found.", json);
                    return;
                }

                if (json)
                {
                    CliOutput.WriteJson(new { deleted = true, id });
                    return;
                }

                AnsiConsole.MarkupLine($"[green]{desc}:[/] {id}");
            }
            catch
            {
                var store = CliOutput.CreateJsonStore();
                if (store is null)
                {
                    CliOutput.WriteError("Database is not available and JSON store could not be created.", json);
                    return;
                }

                CliOutput.WriteError("Database is not available.", json);
            }
        });

        return cmd;
    }
}
