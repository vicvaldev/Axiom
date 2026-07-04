using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Axiom.Application;
using Axiom.Application.Commands;
using Axiom.Application.Dtos;
using Axiom.Application.Interfaces;
using Axiom.Application.Queries;
using Axiom.Domain.Entities;
using Axiom.Infrastructure;
using Axiom.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

Option<bool> NewJsonOption() => new("--json")
{
    Description = "Write machine-readable JSON output"
};

Option<bool> NewWizardOption() => new("--wizard")
{
    Description = "Launch interactive wizard to create the entry"
};

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

var connectionString = Environment.GetEnvironmentVariable("AXIOM_CONNECTION_STRING")
    ?? "Server=localhost;Database=AXIOM;Integrated Security=True;TrustServerCertificate=True;";

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString);

var host = builder.Build();

var rootCommand = new RootCommand("Axiom - KnowledgeOps and Operational Continuity Platform");

var knowledgeCmd = new Command("knowledge", "Manage knowledge entries");

var createCmd = new Command("create", "Create a new knowledge entry");
var titleOpt = new Option<string>("--title") { Required = true };
var summaryOpt = new Option<string>("--summary");
var contentOpt = new Option<string>("--content") { Required = true };
var systemIdOpt = new Option<long>("--system-id");
var systemEaiOpt = new Option<string>("--system-eai");
var typeIdOpt = new Option<long>("--type-id");
var typeCodeOpt = new Option<string>("--type-code");
var stateIdOpt = new Option<int>("--state-id");
var stateCodeOpt = new Option<string>("--state-code");
var userIdOpt = new Option<Guid>("--created-by");
var createdByEmailOpt = new Option<string>("--created-by-email");
var tagsOpt = new Option<string>("--tags");
var issueIdOpt = new Option<Guid?>("--issue-id");
var knowledgeCreateJsonOpt = NewJsonOption();
var knowledgeCreateWizardOpt = NewWizardOption();
createCmd.Options.Add(titleOpt);
createCmd.Options.Add(summaryOpt);
createCmd.Options.Add(contentOpt);
createCmd.Options.Add(systemIdOpt);
createCmd.Options.Add(systemEaiOpt);
createCmd.Options.Add(typeIdOpt);
createCmd.Options.Add(typeCodeOpt);
createCmd.Options.Add(stateIdOpt);
createCmd.Options.Add(stateCodeOpt);
createCmd.Options.Add(userIdOpt);
createCmd.Options.Add(createdByEmailOpt);
createCmd.Options.Add(tagsOpt);
createCmd.Options.Add(issueIdOpt);
createCmd.Options.Add(knowledgeCreateJsonOpt);
createCmd.Options.Add(knowledgeCreateWizardOpt);

createCmd.SetAction((ParseResult result) =>
{
    var wizard = result.GetValue(knowledgeCreateWizardOpt);
    var json = result.GetValue(knowledgeCreateJsonOpt);

    if (wizard)
    {
        using var scope = host.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();

        AnsiConsole.Write(new FigletText("Knowledge Wizard").Color(Color.Cyan));
        AnsiConsole.Write(new Rule("[cyan]Create a new knowledge entry[/]"));

        var users = references.ListUsersAsync().Result;
        var selectedUser = AnsiConsole.Prompt(
            new SelectionPrompt<UserDto>()
                .Title("[cyan]Select user:[/]")
                .PageSize(10)
                .AddChoices(users)
                .UseConverter(u => $"{u.Name} ({u.Email})"));

        var systems = references.ListSystemsAsync().Result;
        var selectedSystem = AnsiConsole.Prompt(
            new SelectionPrompt<SystemDto>()
                .Title("[cyan]Select system:[/]")
                .PageSize(10)
                .AddChoices(systems)
                .UseConverter(s => $"{s.EAI} - {s.Name}"));

        var types = references.ListKnowledgeTypesAsync().Result;
        var selectedType = AnsiConsole.Prompt(
            new SelectionPrompt<ReferenceCodeDto>()
                .Title("[cyan]Select knowledge type:[/]")
                .PageSize(10)
                .AddChoices(types)
                .UseConverter(t => $"{t.Code} - {t.Name}"));

        var states = references.ListKnowledgeStatesAsync().Result.Select(s => new ReferenceCodeDto { Id = s.Id, Code = s.Code, Name = s.Name }).ToList();
        var selectedState = AnsiConsole.Prompt(
            new SelectionPrompt<ReferenceCodeDto>()
                .Title("[cyan]Select knowledge state:[/]")
                .PageSize(10)
                .AddChoices(states)
                .UseConverter(s => $"{s.Code} - {s.Name}"));

        var title = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Title:[/]")
                .Validate(v => string.IsNullOrWhiteSpace(v)
                    ? ValidationResult.Error("[red]Title cannot be empty[/]")
                    : ValidationResult.Success()));

        var summary = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Summary:[/]")
                .AllowEmpty());

        var content = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Content:[/]")
                .Validate(v => string.IsNullOrWhiteSpace(v)
                    ? ValidationResult.Error("[red]Content cannot be empty[/]")
                    : ValidationResult.Success()));

        var tagsInput = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Tags (comma-separated, optional):[/]")
                .AllowEmpty());
        var tagList = string.IsNullOrWhiteSpace(tagsInput)
            ? new List<string>()
            : [.. tagsInput.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

        var issueIdInput = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Issue ID (optional):[/]")
                .AllowEmpty()
                .Validate(v => string.IsNullOrWhiteSpace(v) || Guid.TryParse(v, out _)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be a valid GUID or empty[/]")));
        var issueId = string.IsNullOrWhiteSpace(issueIdInput) ? (Guid?)null : Guid.Parse(issueIdInput);

        var knowledgeId = Guid.NewGuid();

        var summaryTable = new Table();
        summaryTable.AddColumns("Field", "Value");
        summaryTable.AddRow("User", $"{selectedUser.Name} ({selectedUser.Email})");
        summaryTable.AddRow("System", $"{selectedSystem.EAI} - {selectedSystem.Name}");
        summaryTable.AddRow("Type", $"{selectedType.Code} - {selectedType.Name}");
        summaryTable.AddRow("State", $"{selectedState.Code} - {selectedState.Name}");
        summaryTable.AddRow("Title", title);
        summaryTable.AddRow("Summary", summary);
        summaryTable.AddRow("Tags", string.Join(", ", tagList));
        summaryTable.AddRow("Issue ID", issueId?.ToString() ?? "-");
        AnsiConsole.Write(summaryTable);

        if (!AnsiConsole.Confirm("Create this knowledge entry?"))
        {
            AnsiConsole.MarkupLine("[red]Cancelled.[/]");
            return;
        }

        try
        {
            var command = new CreateKnowledgeCommand(
                title, summary, content, selectedSystem.SystemId,
                selectedUser.UserId, selectedType.Id, (int)selectedState.Id,
                issueId, tagList, knowledgeId);

            var entry = mediator.Send(command).Result;

            if (json)
            {
                WriteJson(ToKnowledgeCreateResult(entry));
                return;
            }

            AnsiConsole.MarkupLine($"[green]Knowledge created:[/] {entry.KnowledgeId}");
            AnsiConsole.MarkupLine($"  [bold]Title:[/] {entry.Title}");
            AnsiConsole.MarkupLine($"  [bold]System:[/] {selectedSystem.Name}");
            AnsiConsole.MarkupLine($"  [bold]Type:[/] {selectedType.Name}");
            AnsiConsole.MarkupLine($"  [bold]State:[/] {selectedState.Name}");
        }
        catch
        {
            var store = CreateJsonStore();
            if (store is null)
            {
                WriteError("Database is not available and JSON store could not be created.", json);
                return;
            }

            var jsonEntry = new JsonKnowledgeEntry
            {
                KnowledgeId = knowledgeId, Title = title, Summary = summary, Content = content,
                SystemId = selectedSystem.SystemId, CreatedByUserId = selectedUser.UserId,
                KnowledgeTypeId = selectedType.Id, KnowledgeStateId = (int)selectedState.Id,
                IssueId = issueId, Tags = tagList, VersionNumber = 1,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };

            try
            {
                store.AppendAsync("knowledge", jsonEntry).Wait();

                if (json)
                {
                    WriteJson(jsonEntry);
                    return;
                }

                AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge saved to local store:[/] {knowledgeId}");
                AnsiConsole.MarkupLine($"  [bold]Title:[/] {title}");
                AnsiConsole.MarkupLine($"  [bold]System ID:[/] {selectedSystem.SystemId}");
            }
            catch (Exception storeEx)
            {
                WriteError($"Failed to save to local store: {storeEx.Message}", json);
            }
        }

        return;
    }

    {
        using var scope = host.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
        json = result.GetValue(knowledgeCreateJsonOpt);

    var tags = result.GetValue(tagsOpt);
    var tagList = string.IsNullOrWhiteSpace(tags)
        ? new List<string>()
        : [.. tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

    var systemId = ResolveSystemId(result.GetValue(systemIdOpt), result.GetValue(systemEaiOpt), references, json);
    var typeId = ResolveKnowledgeTypeId(result.GetValue(typeIdOpt), result.GetValue(typeCodeOpt), references, json);
    var stateId = ResolveKnowledgeStateId(result.GetValue(stateIdOpt), result.GetValue(stateCodeOpt), references, json);
    var createdByUserId = ResolveUserId(result.GetValue(userIdOpt), result.GetValue(createdByEmailOpt), references, json);
    if (systemId is null || typeId is null || stateId is null || createdByUserId is null)
    {
        return;
    }

    var title = result.GetValue(titleOpt)!;
    var summary = result.GetValue(summaryOpt) ?? string.Empty;
    var content = result.GetValue(contentOpt)!;
    var issueId = result.GetValue(issueIdOpt);
    var knowledgeId = Guid.NewGuid();

    try
    {
        var command = new CreateKnowledgeCommand(
            title,
            summary,
            content,
            systemId.Value,
            createdByUserId.Value,
            typeId.Value,
            stateId.Value,
            issueId,
            tagList,
            knowledgeId);

        var entry = mediator.Send(command).Result;

        if (json)
        {
            WriteJson(ToKnowledgeCreateResult(entry));
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge created:[/] {entry.KnowledgeId}");
        AnsiConsole.MarkupLine($"  [bold]Title:[/] {entry.Title}");
        AnsiConsole.MarkupLine($"  [bold]System ID:[/] {entry.SystemId}");
        AnsiConsole.MarkupLine($"  [bold]Type ID:[/] {entry.KnowledgeTypeId}");
        AnsiConsole.MarkupLine($"  [bold]State ID:[/] {entry.KnowledgeStateId}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var jsonEntry = new JsonKnowledgeEntry
        {
            KnowledgeId = knowledgeId,
            Title = title,
            Summary = summary,
            Content = content,
            SystemId = systemId.Value,
            CreatedByUserId = createdByUserId.Value,
            KnowledgeTypeId = typeId.Value,
            KnowledgeStateId = stateId.Value,
            IssueId = issueId,
            Tags = tagList,
            VersionNumber = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            store.AppendAsync("knowledge", jsonEntry).Wait();

            if (json)
            {
                WriteJson(jsonEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge saved to local store:[/] {knowledgeId}");
            AnsiConsole.MarkupLine($"  [bold]Title:[/] {title}");
            AnsiConsole.MarkupLine($"  [bold]System ID:[/] {systemId}");
            AnsiConsole.MarkupLine($"  [bold]Type ID:[/] {typeId}");
            AnsiConsole.MarkupLine($"  [bold]State ID:[/] {stateId}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to save to local store: {storeEx.Message}", json);
        }
    }
    }
});

knowledgeCmd.Subcommands.Add(createCmd);

var knowledgeUpdateCmd = new Command("update", "Update a knowledge entry");
var knowledgeUpdateIdArg = new Argument<Guid>("id");
var knowledgeUpdateTitleOpt = new Option<string>("--title") { Required = true };
var knowledgeUpdateSummaryOpt = new Option<string>("--summary");
var knowledgeUpdateContentOpt = new Option<string>("--content") { Required = true };
var knowledgeUpdateSystemIdOpt = new Option<long>("--system-id");
var knowledgeUpdateSystemEaiOpt = new Option<string>("--system-eai");
var knowledgeUpdateTypeIdOpt = new Option<long>("--type-id");
var knowledgeUpdateTypeCodeOpt = new Option<string>("--type-code");
var knowledgeUpdateStateIdOpt = new Option<int>("--state-id");
var knowledgeUpdateStateCodeOpt = new Option<string>("--state-code");
var knowledgeUpdateTagsOpt = new Option<string>("--tags");
var knowledgeUpdateIssueIdOpt = new Option<Guid?>("--issue-id");
var knowledgeUpdateJsonOpt = NewJsonOption();
knowledgeUpdateCmd.Arguments.Add(knowledgeUpdateIdArg);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateTitleOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateSummaryOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateContentOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateSystemIdOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateSystemEaiOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateTypeIdOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateTypeCodeOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateStateIdOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateStateCodeOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateTagsOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateIssueIdOpt);
knowledgeUpdateCmd.Options.Add(knowledgeUpdateJsonOpt);

knowledgeUpdateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(knowledgeUpdateJsonOpt);

    var id = result.GetValue(knowledgeUpdateIdArg);
    var tags = result.GetValue(knowledgeUpdateTagsOpt);
    var tagList = string.IsNullOrWhiteSpace(tags)
        ? new List<string>()
        : [.. tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

    var systemId = ResolveSystemId(result.GetValue(knowledgeUpdateSystemIdOpt), result.GetValue(knowledgeUpdateSystemEaiOpt), references, json);
    var typeId = ResolveKnowledgeTypeId(result.GetValue(knowledgeUpdateTypeIdOpt), result.GetValue(knowledgeUpdateTypeCodeOpt), references, json);
    var stateId = ResolveKnowledgeStateId(result.GetValue(knowledgeUpdateStateIdOpt), result.GetValue(knowledgeUpdateStateCodeOpt), references, json);
    if (systemId is null || typeId is null || stateId is null)
    {
        return;
    }

    var title = result.GetValue(knowledgeUpdateTitleOpt)!;
    var summary = result.GetValue(knowledgeUpdateSummaryOpt) ?? string.Empty;
    var content = result.GetValue(knowledgeUpdateContentOpt)!;
    var issueId = result.GetValue(knowledgeUpdateIssueIdOpt);

    try
    {
        var command = new UpdateKnowledgeCommand(
            id,
            title,
            summary,
            content,
            systemId.Value,
            typeId.Value,
            stateId.Value,
            issueId,
            tagList);

        var entry = mediator.Send(command).Result;

        if (entry is null)
        {
            WriteError("Knowledge entry not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new
            {
                entry.KnowledgeId,
                entry.Title,
                entry.Summary,
                entry.Content,
                entry.SystemId,
                entry.KnowledgeTypeId,
                entry.KnowledgeStateId,
                entry.IssueId,
                Tags = tagList,
                entry.VersionNumber,
                entry.UpdatedAt
            });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge updated:[/] {entry.KnowledgeId}");
        AnsiConsole.MarkupLine($"  [bold]Title:[/] {entry.Title}");
        AnsiConsole.MarkupLine($"  [bold]System ID:[/] {entry.SystemId}");
        AnsiConsole.MarkupLine($"  [bold]Type ID:[/] {entry.KnowledgeTypeId}");
        AnsiConsole.MarkupLine($"  [bold]State ID:[/] {entry.KnowledgeStateId}");
        AnsiConsole.MarkupLine($"  [bold]Version:[/] {entry.VersionNumber}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var existingEntry = store.FindByIdAsync<JsonKnowledgeEntry>("knowledge", e => e.KnowledgeId == id).Result;
        if (existingEntry is null)
        {
            WriteError("Knowledge entry not found.", json);
            return;
        }

        var updatedEntry = new JsonKnowledgeEntry
        {
            KnowledgeId = id,
            Title = title,
            Summary = summary,
            Content = content,
            SystemId = systemId.Value,
            CreatedByUserId = existingEntry.CreatedByUserId,
            KnowledgeTypeId = typeId.Value,
            KnowledgeStateId = stateId.Value,
            IssueId = issueId,
            Tags = tagList,
            VersionNumber = existingEntry.VersionNumber + 1,
            CreatedAt = existingEntry.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            store.UpdateAsync("knowledge", e => e.KnowledgeId == id, updatedEntry).Wait();

            if (json)
            {
                WriteJson(updatedEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge updated in local store:[/] {id}");
            AnsiConsole.MarkupLine($"  [bold]Title:[/] {title}");
            AnsiConsole.MarkupLine($"  [bold]System ID:[/] {systemId}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to update in local store: {storeEx.Message}", json);
        }
    }
});

knowledgeCmd.Subcommands.Add(knowledgeUpdateCmd);

var listCmd = new Command("list", "List all knowledge entries");
var knowledgeListJsonOpt = NewJsonOption();
listCmd.Options.Add(knowledgeListJsonOpt);
listCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeListJsonOpt);

    try
    {
        var entries = mediator.Send(new ListKnowledgeQuery()).Result;
        if (json)
        {
            WriteJson(entries);
            return;
        }

        var table = new Table();
        table.AddColumns("Id", "Title", "System", "Type", "State", "Tags", "Updated");

        foreach (var entry in entries)
        {
            table.AddRow(
                entry.KnowledgeId.ToString()[..8],
                entry.Title,
                entry.SystemName,
                entry.TypeName,
                entry.StateName,
                string.Join(", ", entry.Tags),
                entry.UpdatedAt.ToString("yyyy-MM-dd"));
        }

        AnsiConsole.Write(table);
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entries = store.ReadAllAsync<JsonKnowledgeEntry>("knowledge").Result;

        if (json)
        {
            WriteJson(entries);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var table = new Table();
        table.AddColumns("Id", "Title", "SystemId", "TypeId", "StateId", "Tags", "Updated");

        foreach (var entry in entries)
        {
            table.AddRow(
                entry.KnowledgeId.ToString()[..8],
                entry.Title,
                entry.SystemId.ToString(),
                entry.KnowledgeTypeId.ToString(),
                entry.KnowledgeStateId.ToString(),
                string.Join(", ", entry.Tags),
                entry.UpdatedAt.ToString("yyyy-MM-dd"));
        }

        AnsiConsole.Write(table);
    }
});

knowledgeCmd.Subcommands.Add(listCmd);

var showCmd = new Command("show", "Show knowledge entry details");
var idArg = new Argument<Guid>("id");
var knowledgeShowJsonOpt = NewJsonOption();
showCmd.Arguments.Add(idArg);
showCmd.Options.Add(knowledgeShowJsonOpt);
showCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var id = result.GetValue(idArg);
    var json = result.GetValue(knowledgeShowJsonOpt);

    try
    {
        var entry = mediator.Send(new GetKnowledgeByIdQuery(id)).Result;

        if (entry is null)
        {
            WriteError("Knowledge entry not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(ToKnowledgeDetails(entry));
            return;
        }

        var panel = new Panel(
            new Markup(
                $"[bold]Title:[/] {entry.Title}\n" +
                $"[bold]Summary:[/] {entry.Summary}\n" +
                $"[bold]System:[/] {entry.System?.Name ?? entry.SystemId.ToString()}\n" +
                $"[bold]Type:[/] {entry.Type?.Name ?? entry.KnowledgeTypeId.ToString()}\n" +
                $"[bold]State:[/] {entry.State?.Name ?? entry.KnowledgeStateId.ToString()}\n" +
                $"[bold]Created By:[/] {entry.CreatedBy?.Name ?? entry.CreatedByUserId.ToString()}\n" +
                $"[bold]Tags:[/] {string.Join(", ", entry.KnowledgeKnowledgeTags?.Select(t => t.Tag?.TagName ?? string.Empty) ?? [])}\n" +
                $"[bold]Version:[/] {entry.VersionNumber}\n" +
                $"[bold]Created:[/] {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                $"[bold]Updated:[/] {entry.UpdatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                $"[bold]Content:[/]\n{entry.Content}"))
        {
            Header = new PanelHeader($"Knowledge - {entry.KnowledgeId}")
        };

        AnsiConsole.Write(panel);
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entry = store.FindByIdAsync<JsonKnowledgeEntry>("knowledge", e => e.KnowledgeId == id).Result;
        if (entry is null)
        {
            WriteError("Knowledge entry not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(entry);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var panel = new Panel(
            new Markup(
                $"[bold]Title:[/] {entry.Title}\n" +
                $"[bold]Summary:[/] {entry.Summary}\n" +
                $"[bold]System ID:[/] {entry.SystemId}\n" +
                $"[bold]Type ID:[/] {entry.KnowledgeTypeId}\n" +
                $"[bold]State ID:[/] {entry.KnowledgeStateId}\n" +
                $"[bold]Created By User ID:[/] {entry.CreatedByUserId}\n" +
                $"[bold]Tags:[/] {string.Join(", ", entry.Tags)}\n" +
                $"[bold]Version:[/] {entry.VersionNumber}\n" +
                $"[bold]Created:[/] {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                $"[bold]Updated:[/] {entry.UpdatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                $"[bold]Content:[/]\n{entry.Content}"))
        {
            Header = new PanelHeader($"Knowledge - {entry.KnowledgeId} (local store)")
        };

        AnsiConsole.Write(panel);
    }
});

knowledgeCmd.Subcommands.Add(showCmd);

var searchCmd = new Command("search", "Search knowledge entries");
var queryArg = new Argument<string>("query");
var knowledgeSearchJsonOpt = NewJsonOption();
searchCmd.Arguments.Add(queryArg);
searchCmd.Options.Add(knowledgeSearchJsonOpt);
searchCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeSearchJsonOpt);

    var query = result.GetValue(queryArg) ?? string.Empty;

    try
    {
        var entries = mediator.Send(new SearchKnowledgeQuery(query)).Result;
        if (json)
        {
            WriteJson(entries);
            return;
        }

        var table = new Table();
        table.AddColumns("Id", "Title", "System", "Type", "State");

        foreach (var entry in entries)
        {
            table.AddRow(
                entry.KnowledgeId.ToString()[..8],
                entry.Title,
                entry.SystemName,
                entry.TypeName,
                entry.StateName);
        }

        AnsiConsole.Write(table);
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var allEntries = store.ReadAllAsync<JsonKnowledgeEntry>("knowledge").Result;
        var q = query.ToLowerInvariant();
        var matches = allEntries
            .Where(e => e.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                || e.Summary.Contains(q, StringComparison.OrdinalIgnoreCase)
                || e.Content.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (json)
        {
            WriteJson(matches);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var table = new Table();
        table.AddColumns("Id", "Title", "SystemId", "TypeId", "StateId");

        foreach (var entry in matches)
        {
            table.AddRow(
                entry.KnowledgeId.ToString()[..8],
                entry.Title,
                entry.SystemId.ToString(),
                entry.KnowledgeTypeId.ToString(),
                entry.KnowledgeStateId.ToString());
        }

        AnsiConsole.Write(table);
    }
});

knowledgeCmd.Subcommands.Add(searchCmd);
rootCommand.Subcommands.Add(knowledgeCmd);

var issueCmd = new Command("issue", "Manage issue records");

var issueCreateCmd = new Command("create", "Create a new issue record");
var issueSummaryOpt = new Option<string>("--summary") { Required = true };
var issueSystemIdOpt = new Option<long>("--system-id");
var issueSystemEaiOpt = new Option<string>("--system-eai");
var problemOpt = new Option<string>("--problem") { Required = true };
var analysisOpt = new Option<string>("--analysis");
var resolutionOpt = new Option<string>("--resolution");
var issueStateIdOpt = new Option<int>("--state-id");
var issueStateCodeOpt = new Option<string>("--state-code");
var issueUserIdOpt = new Option<Guid>("--created-by");
var issueCreatedByEmailOpt = new Option<string>("--created-by-email");
var ritmOpt = new Option<string>("--ritm-number");
var incidentOpt = new Option<string>("--incident-number");
var issueCreateJsonOpt = NewJsonOption();
var issueCreateWizardOpt = NewWizardOption();
issueCreateCmd.Options.Add(issueSummaryOpt);
issueCreateCmd.Options.Add(issueSystemIdOpt);
issueCreateCmd.Options.Add(issueSystemEaiOpt);
issueCreateCmd.Options.Add(problemOpt);
issueCreateCmd.Options.Add(analysisOpt);
issueCreateCmd.Options.Add(resolutionOpt);
issueCreateCmd.Options.Add(issueStateIdOpt);
issueCreateCmd.Options.Add(issueStateCodeOpt);
issueCreateCmd.Options.Add(issueUserIdOpt);
issueCreateCmd.Options.Add(issueCreatedByEmailOpt);
issueCreateCmd.Options.Add(ritmOpt);
issueCreateCmd.Options.Add(incidentOpt);
issueCreateCmd.Options.Add(issueCreateJsonOpt);
issueCreateCmd.Options.Add(issueCreateWizardOpt);

issueCreateCmd.SetAction((ParseResult result) =>
{
    var wizard = result.GetValue(issueCreateWizardOpt);
    var json = result.GetValue(issueCreateJsonOpt);

    if (wizard)
    {
        using var scope = host.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();

        AnsiConsole.Write(new FigletText("Issue Wizard").Color(Color.Yellow));
        AnsiConsole.Write(new Rule("[yellow]Create a new issue[/]"));

        var users = references.ListUsersAsync().Result;
        var selectedUser = AnsiConsole.Prompt(
            new SelectionPrompt<UserDto>()
                .Title("[cyan]Select user:[/]")
                .PageSize(10)
                .AddChoices(users)
                .UseConverter(u => $"{u.Name} ({u.Email})"));

        var systems = references.ListSystemsAsync().Result;
        var selectedSystem = AnsiConsole.Prompt(
            new SelectionPrompt<SystemDto>()
                .Title("[cyan]Select system:[/]")
                .PageSize(10)
                .AddChoices(systems)
                .UseConverter(s => $"{s.EAI} - {s.Name}"));

        var states = references.ListIssueStatesAsync().Result.Select(s => new ReferenceCodeDto { Id = s.Id, Code = s.Code, Name = s.Name }).ToList();
        var selectedState = AnsiConsole.Prompt(
            new SelectionPrompt<ReferenceCodeDto>()
                .Title("[cyan]Select issue state:[/]")
                .PageSize(10)
                .AddChoices(states)
                .UseConverter(s => $"{s.Code} - {s.Name}"));

        var summary = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Summary:[/]")
                .Validate(v => string.IsNullOrWhiteSpace(v)
                    ? ValidationResult.Error("[red]Summary cannot be empty[/]")
                    : ValidationResult.Success()));

        var problem = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Problem description:[/]")
                .Validate(v => string.IsNullOrWhiteSpace(v)
                    ? ValidationResult.Error("[red]Problem cannot be empty[/]")
                    : ValidationResult.Success()));

        var analysis = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Analysis (optional):[/]")
                .AllowEmpty());

        var resolution = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Resolution (optional):[/]")
                .AllowEmpty());

        var ritmNumber = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]RITM number (optional):[/]")
                .AllowEmpty());

        var ritmValue = string.IsNullOrWhiteSpace(ritmNumber) ? null : ritmNumber;

        var incidentNumber = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Incident number (optional):[/]")
                .AllowEmpty());

        var incidentValue = string.IsNullOrWhiteSpace(incidentNumber) ? null : incidentNumber;

        var issueId = Guid.NewGuid();

        var summaryTable = new Table();
        summaryTable.AddColumns("Field", "Value");
        summaryTable.AddRow("User", $"{selectedUser.Name} ({selectedUser.Email})");
        summaryTable.AddRow("System", $"{selectedSystem.EAI} - {selectedSystem.Name}");
        summaryTable.AddRow("State", $"{selectedState.Code} - {selectedState.Name}");
        summaryTable.AddRow("Summary", summary);
        summaryTable.AddRow("RITM", ritmValue ?? "-");
        summaryTable.AddRow("Incident", incidentValue ?? "-");
        AnsiConsole.Write(summaryTable);

        if (!AnsiConsole.Confirm("Create this issue?"))
        {
            AnsiConsole.MarkupLine("[red]Cancelled.[/]");
            return;
        }

        try
        {
            var command = new CreateIssueCommand(
                summary, selectedSystem.SystemId, problem, analysis, resolution,
                (int)selectedState.Id, selectedUser.UserId, ritmValue, incidentValue, issueId);

            var issue = mediator.Send(command).Result;

            if (json)
            {
                WriteJson(ToIssueCreateResult(issue));
                return;
            }

            AnsiConsole.MarkupLine($"[green]Issue created:[/] {issue.IssueId}");
            AnsiConsole.MarkupLine($"  [bold]Summary:[/] {issue.Summary}");
            AnsiConsole.MarkupLine($"  [bold]System:[/] {selectedSystem.Name}");
            AnsiConsole.MarkupLine($"  [bold]State:[/] {selectedState.Name}");
        }
        catch
        {
            var store = CreateJsonStore();
            if (store is null)
            {
                WriteError("Database is not available and JSON store could not be created.", json);
                return;
            }

            var jsonEntry = new JsonIssueEntry
            {
                IssueId = issueId, Summary = summary, SystemId = selectedSystem.SystemId,
                Problem = problem, Analysis = analysis, Resolution = resolution,
                StateId = (int)selectedState.Id, CreatedByUserId = selectedUser.UserId,
                RitmNumber = ritmValue, IncidentNumber = incidentValue,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };

            try
            {
                store.AppendAsync("issues", jsonEntry).Wait();

                if (json)
                {
                    WriteJson(jsonEntry);
                    return;
                }

                AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue saved to local store:[/] {issueId}");
                AnsiConsole.MarkupLine($"  [bold]Summary:[/] {summary}");
                AnsiConsole.MarkupLine($"  [bold]System ID:[/] {selectedSystem.SystemId}");
            }
            catch (Exception storeEx)
            {
                WriteError($"Failed to save to local store: {storeEx.Message}", json);
            }
        }

        return;
    }

    {
        using var scope = host.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
        json = result.GetValue(issueCreateJsonOpt);

    var systemId = ResolveSystemId(result.GetValue(issueSystemIdOpt), result.GetValue(issueSystemEaiOpt), references, json);
    var stateId = ResolveIssueStateId(result.GetValue(issueStateIdOpt), result.GetValue(issueStateCodeOpt), references, json);
    var createdByUserId = ResolveUserId(result.GetValue(issueUserIdOpt), result.GetValue(issueCreatedByEmailOpt), references, json);
    if (systemId is null || stateId is null || createdByUserId is null)
    {
        return;
    }

    var summary = result.GetValue(issueSummaryOpt)!;
    var problem = result.GetValue(problemOpt)!;
    var analysis = result.GetValue(analysisOpt) ?? string.Empty;
    var resolution = result.GetValue(resolutionOpt) ?? string.Empty;
    var ritmNumber = result.GetValue(ritmOpt);
    var incidentNumber = result.GetValue(incidentOpt);
    var issueId = Guid.NewGuid();

    try
    {
        var command = new CreateIssueCommand(
            summary,
            systemId.Value,
            problem,
            analysis,
            resolution,
            stateId.Value,
            createdByUserId.Value,
            ritmNumber,
            incidentNumber,
            issueId);

        var issue = mediator.Send(command).Result;

        if (json)
        {
            WriteJson(ToIssueCreateResult(issue));
            return;
        }

        AnsiConsole.MarkupLine($"[green]Issue created:[/] {issue.IssueId}");
        AnsiConsole.MarkupLine($"  [bold]Summary:[/] {issue.Summary}");
        AnsiConsole.MarkupLine($"  [bold]System ID:[/] {issue.SystemId}");
        AnsiConsole.MarkupLine($"  [bold]State ID:[/] {issue.StateId}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var jsonEntry = new JsonIssueEntry
        {
            IssueId = issueId,
            Summary = summary,
            SystemId = systemId.Value,
            Problem = problem,
            Analysis = analysis,
            Resolution = resolution,
            StateId = stateId.Value,
            CreatedByUserId = createdByUserId.Value,
            RitmNumber = ritmNumber,
            IncidentNumber = incidentNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            store.AppendAsync("issues", jsonEntry).Wait();

            if (json)
            {
                WriteJson(jsonEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue saved to local store:[/] {issueId}");
            AnsiConsole.MarkupLine($"  [bold]Summary:[/] {summary}");
            AnsiConsole.MarkupLine($"  [bold]System ID:[/] {systemId}");
            AnsiConsole.MarkupLine($"  [bold]State ID:[/] {stateId}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to save to local store: {storeEx.Message}", json);
        }
    }
    }
});

issueCmd.Subcommands.Add(issueCreateCmd);

var issueUpdateCmd = new Command("update", "Update an issue record");
var issueUpdateIdArg = new Argument<Guid>("id");
var issueUpdateSummaryOpt = new Option<string>("--summary") { Required = true };
var issueUpdateSystemIdOpt = new Option<long>("--system-id");
var issueUpdateSystemEaiOpt = new Option<string>("--system-eai");
var issueUpdateProblemOpt = new Option<string>("--problem") { Required = true };
var issueUpdateAnalysisOpt = new Option<string>("--analysis");
var issueUpdateResolutionOpt = new Option<string>("--resolution");
var issueUpdateStateIdOpt = new Option<int>("--state-id");
var issueUpdateStateCodeOpt = new Option<string>("--state-code");
var issueUpdateRitmOpt = new Option<string>("--ritm-number");
var issueUpdateIncidentOpt = new Option<string>("--incident-number");
var issueUpdateJsonOpt = NewJsonOption();
issueUpdateCmd.Arguments.Add(issueUpdateIdArg);
issueUpdateCmd.Options.Add(issueUpdateSummaryOpt);
issueUpdateCmd.Options.Add(issueUpdateSystemIdOpt);
issueUpdateCmd.Options.Add(issueUpdateSystemEaiOpt);
issueUpdateCmd.Options.Add(issueUpdateProblemOpt);
issueUpdateCmd.Options.Add(issueUpdateAnalysisOpt);
issueUpdateCmd.Options.Add(issueUpdateResolutionOpt);
issueUpdateCmd.Options.Add(issueUpdateStateIdOpt);
issueUpdateCmd.Options.Add(issueUpdateStateCodeOpt);
issueUpdateCmd.Options.Add(issueUpdateRitmOpt);
issueUpdateCmd.Options.Add(issueUpdateIncidentOpt);
issueUpdateCmd.Options.Add(issueUpdateJsonOpt);

issueUpdateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(issueUpdateJsonOpt);

    var id = result.GetValue(issueUpdateIdArg);
    var systemId = ResolveSystemId(result.GetValue(issueUpdateSystemIdOpt), result.GetValue(issueUpdateSystemEaiOpt), references, json);
    var stateId = ResolveIssueStateId(result.GetValue(issueUpdateStateIdOpt), result.GetValue(issueUpdateStateCodeOpt), references, json);
    if (systemId is null || stateId is null)
    {
        return;
    }

    var summary = result.GetValue(issueUpdateSummaryOpt)!;
    var problem = result.GetValue(issueUpdateProblemOpt)!;
    var analysis = result.GetValue(issueUpdateAnalysisOpt) ?? string.Empty;
    var resolution = result.GetValue(issueUpdateResolutionOpt) ?? string.Empty;
    var ritmNumber = result.GetValue(issueUpdateRitmOpt);
    var incidentNumber = result.GetValue(issueUpdateIncidentOpt);

    try
    {
        var command = new UpdateIssueCommand(
            id,
            summary,
            problem,
            analysis,
            resolution,
            systemId.Value,
            stateId.Value,
            ritmNumber,
            incidentNumber);

        var issue = mediator.Send(command).Result;

        if (issue is null)
        {
            WriteError("Issue not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new
            {
                issue.IssueId,
                issue.Summary,
                issue.SystemId,
                issue.StateId,
                issue.RitmNumber,
                issue.IncidentNumber,
                issue.Problem,
                issue.Analysis,
                issue.Resolution,
                issue.UpdatedAt
            });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Issue updated:[/] {issue.IssueId}");
        AnsiConsole.MarkupLine($"  [bold]Summary:[/] {issue.Summary}");
        AnsiConsole.MarkupLine($"  [bold]System ID:[/] {issue.SystemId}");
        AnsiConsole.MarkupLine($"  [bold]State ID:[/] {issue.StateId}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var existingEntry = store.FindByIdAsync<JsonIssueEntry>("issues", e => e.IssueId == id).Result;
        if (existingEntry is null)
        {
            WriteError("Issue not found.", json);
            return;
        }

        var updatedEntry = new JsonIssueEntry
        {
            IssueId = id,
            Summary = summary,
            SystemId = systemId.Value,
            Problem = problem,
            Analysis = analysis,
            Resolution = resolution,
            StateId = stateId.Value,
            CreatedByUserId = existingEntry.CreatedByUserId,
            RitmNumber = ritmNumber,
            IncidentNumber = incidentNumber,
            CreatedAt = existingEntry.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            ResolvedAt = existingEntry.ResolvedAt
        };

        try
        {
            store.UpdateAsync("issues", e => e.IssueId == id, updatedEntry).Wait();

            if (json)
            {
                WriteJson(updatedEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue updated in local store:[/] {id}");
            AnsiConsole.MarkupLine($"  [bold]Summary:[/] {summary}");
            AnsiConsole.MarkupLine($"  [bold]System ID:[/] {systemId}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to update in local store: {storeEx.Message}", json);
        }
    }
});

issueCmd.Subcommands.Add(issueUpdateCmd);

var eaiOpt = new Option<string>("--eai")
{
    Description = "Filter by system EAI code"
};
var issueListCmd = new Command("list", "List all issues");
var issueListJsonOpt = NewJsonOption();
issueListCmd.Options.Add(eaiOpt);
issueListCmd.Options.Add(issueListJsonOpt);
issueListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(issueListJsonOpt);

    var eai = result.GetValue(eaiOpt);

    try
    {
        var issues = mediator.Send(new ListIssuesQuery(eai)).Result;
        if (json)
        {
            WriteJson(issues);
            return;
        }

        var table = new Table();
        table.AddColumns("Id", "Summary", "System", "State", "RITM", "Incident", "Created");

        foreach (var issue in issues)
        {
            table.AddRow(
                issue.IssueId.ToString()[..8],
                issue.Summary,
                issue.SystemName,
                issue.StateName,
                issue.RitmNumber ?? "-",
                issue.IncidentNumber ?? "-",
                issue.CreatedAt.ToString("yyyy-MM-dd"));
        }

        AnsiConsole.Write(table);
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entries = store.ReadAllAsync<JsonIssueEntry>("issues").Result;

        if (json)
        {
            WriteJson(entries);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var table = new Table();
        table.AddColumns("Id", "Summary", "SystemId", "StateId", "RITM", "Incident", "Created");

        foreach (var entry in entries)
        {
            table.AddRow(
                entry.IssueId.ToString()[..8],
                entry.Summary,
                entry.SystemId.ToString(),
                entry.StateId.ToString(),
                entry.RitmNumber ?? "-",
                entry.IncidentNumber ?? "-",
                entry.CreatedAt.ToString("yyyy-MM-dd"));
        }

        AnsiConsole.Write(table);
    }
});

issueCmd.Subcommands.Add(issueListCmd);

var issueShowCmd = new Command("show", "Show issue record details");
var issueIdArg = new Argument<Guid>("id");
var issueShowJsonOpt = NewJsonOption();
issueShowCmd.Arguments.Add(issueIdArg);
issueShowCmd.Options.Add(issueShowJsonOpt);
issueShowCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var id = result.GetValue(issueIdArg);
    var json = result.GetValue(issueShowJsonOpt);

    try
    {
        var issue = mediator.Send(new GetIssueByIdQuery(id)).Result;

        if (issue is null)
        {
            WriteError("Issue not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(ToIssueDetails(issue));
            return;
        }

        var panel = new Panel(
            new Markup(
                $"[bold]Summary:[/] {issue.Summary}\n" +
                $"[bold]System:[/] {issue.System?.Name ?? issue.SystemId.ToString()}\n" +
                $"[bold]State:[/] {issue.State?.Name ?? issue.StateId.ToString()}\n" +
                $"[bold]RITM:[/] {issue.RitmNumber ?? "-"}\n" +
                $"[bold]Incident:[/] {issue.IncidentNumber ?? "-"}\n" +
                $"[bold]Created By:[/] {issue.CreatedBy?.Name ?? issue.CreatedByUserId.ToString()}\n" +
                $"[bold]Created:[/] {issue.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                $"[bold]Resolved:[/] {(issue.ResolvedAt.HasValue ? issue.ResolvedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "-")}\n" +
                $"[bold]Problem:[/]\n{issue.Problem}\n\n" +
                $"[bold]Analysis:[/]\n{issue.Analysis}\n\n" +
                $"[bold]Resolution:[/]\n{issue.Resolution}"))
        {
            Header = new PanelHeader($"Issue - {issue.IssueId}")
        };

        AnsiConsole.Write(panel);
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entry = store.FindByIdAsync<JsonIssueEntry>("issues", e => e.IssueId == id).Result;
        if (entry is null)
        {
            WriteError("Issue not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(entry);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var panel = new Panel(
            new Markup(
                $"[bold]Summary:[/] {entry.Summary}\n" +
                $"[bold]System ID:[/] {entry.SystemId}\n" +
                $"[bold]State ID:[/] {entry.StateId}\n" +
                $"[bold]RITM:[/] {entry.RitmNumber ?? "-"}\n" +
                $"[bold]Incident:[/] {entry.IncidentNumber ?? "-"}\n" +
                $"[bold]Created By User ID:[/] {entry.CreatedByUserId}\n" +
                $"[bold]Created:[/] {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
                $"[bold]Resolved:[/] {(entry.ResolvedAt.HasValue ? entry.ResolvedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "-")}\n" +
                $"[bold]Problem:[/]\n{entry.Problem}\n\n" +
                $"[bold]Analysis:[/]\n{entry.Analysis}\n\n" +
                $"[bold]Resolution:[/]\n{entry.Resolution}"))
        {
            Header = new PanelHeader($"Issue - {entry.IssueId} (local store)")
        };

        AnsiConsole.Write(panel);
    }
});

issueCmd.Subcommands.Add(issueShowCmd);

var issueDeleteCmd = new Command("delete", "Delete an issue record");
var issueDeleteIdArg = new Argument<Guid>("id");
var issueDeleteJsonOpt = NewJsonOption();
issueDeleteCmd.Arguments.Add(issueDeleteIdArg);
issueDeleteCmd.Options.Add(issueDeleteJsonOpt);

issueDeleteCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(issueDeleteJsonOpt);

    var id = result.GetValue(issueDeleteIdArg);

    try
    {
        var ok = mediator.Send(new DeleteIssueCommand(id)).Result;
        if (!ok)
        {
            WriteError("Issue not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new { deleted = true, id });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Issue deleted:[/] {id}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        try
        {
            var ok = store.DeleteAsync<JsonIssueEntry>("issues", e => e.IssueId == id).Result;
            if (!ok)
            {
                WriteError("Issue not found.", json);
                return;
            }

            if (json)
            {
                WriteJson(new { deleted = true, id });
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue deleted from local store:[/] {id}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to delete in local store: {storeEx.Message}", json);
        }
    }
});

issueCmd.Subcommands.Add(issueDeleteCmd);
rootCommand.Subcommands.Add(issueCmd);

var userCmd = new Command("user", "Manage users");
var userListCmd = new Command("list", "List users");
var userListJsonOpt = NewJsonOption();
userListCmd.Options.Add(userListJsonOpt);
userListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(userListJsonOpt);

    try
    {
        var users = references.ListUsersAsync().Result;
        if (json)
        {
            WriteJson(users);
            return;
        }

        var table = new Table();
        table.AddColumns("Id", "Email", "Name");
        foreach (var user in users)
        {
            table.AddRow(user.UserId.ToString()[..8], user.Email, user.Name);
        }

        AnsiConsole.Write(table);
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entries = store.ReadAllAsync<JsonUserEntry>("users").Result;
        if (json)
        {
            WriteJson(entries);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var table = new Table();
        table.AddColumns("Id", "Email", "Name");
        foreach (var entry in entries)
        {
            table.AddRow(entry.UserId.ToString()[..8], entry.Email, entry.Name);
        }

        AnsiConsole.Write(table);
    }
});
userCmd.Subcommands.Add(userListCmd);

var userUpdateCmd = new Command("update", "Update a user");
var userUpdateIdArg = new Argument<Guid>("id");
var userUpdateEmailOpt = new Option<string>("--email") { Required = true };
var userUpdateNameOpt = new Option<string>("--name") { Required = true };
var userUpdateJsonOpt = NewJsonOption();
userUpdateCmd.Arguments.Add(userUpdateIdArg);
userUpdateCmd.Options.Add(userUpdateEmailOpt);
userUpdateCmd.Options.Add(userUpdateNameOpt);
userUpdateCmd.Options.Add(userUpdateJsonOpt);

userUpdateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(userUpdateJsonOpt);

    var id = result.GetValue(userUpdateIdArg);
    var email = result.GetValue(userUpdateEmailOpt)!;
    var name = result.GetValue(userUpdateNameOpt)!;

    try
    {
        var command = new UpdateUserCommand(id, email, name);
        var entry = mediator.Send(command).Result;

        if (entry is null)
        {
            WriteError("User not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new
            {
                entry.UserId,
                entry.Email,
                entry.Name
            });
            return;
        }

        AnsiConsole.MarkupLine($"[green]User updated:[/] {entry.UserId}");
        AnsiConsole.MarkupLine($"  [bold]Email:[/] {entry.Email}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var existingEntry = store.FindByIdAsync<JsonUserEntry>("users", e => e.UserId == id).Result;
        if (existingEntry is null)
        {
            WriteError("User not found.", json);
            return;
        }

        var updatedEntry = new JsonUserEntry
        {
            UserId = id,
            Email = email,
            Name = name
        };

        try
        {
            store.UpdateAsync("users", e => e.UserId == id, updatedEntry).Wait();

            if (json)
            {
                WriteJson(updatedEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - User updated in local store:[/] {id}");
            AnsiConsole.MarkupLine($"  [bold]Email:[/] {email}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to update in local store: {storeEx.Message}", json);
        }
    }
});

userCmd.Subcommands.Add(userUpdateCmd);

var userCreateCmd = new Command("create", "Create a new user");
var userCreateEmailOpt = new Option<string>("--email") { Required = true };
var userCreateNameOpt = new Option<string>("--name") { Required = true };
var userCreateJsonOpt = NewJsonOption();
userCreateCmd.Options.Add(userCreateEmailOpt);
userCreateCmd.Options.Add(userCreateNameOpt);
userCreateCmd.Options.Add(userCreateJsonOpt);

userCreateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(userCreateJsonOpt);

    var email = result.GetValue(userCreateEmailOpt)!;
    var name = result.GetValue(userCreateNameOpt)!;

    try
    {
        var entry = mediator.Send(new CreateUserCommand(email, name)).Result;

        if (json)
        {
            WriteJson(new { entry.UserId, entry.Email, entry.Name });
            return;
        }

        AnsiConsole.MarkupLine($"[green]User created:[/] {entry.UserId}");
        AnsiConsole.MarkupLine($"  [bold]Email:[/] {entry.Email}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var userId = Guid.NewGuid();
        var jsonEntry = new JsonUserEntry
        {
            UserId = userId,
            Email = email,
            Name = name
        };

        try
        {
            store.AppendAsync("users", jsonEntry).Wait();

            if (json)
            {
                WriteJson(jsonEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - User saved to local store:[/] {userId}");
            AnsiConsole.MarkupLine($"  [bold]Email:[/] {email}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to save to local store: {storeEx.Message}", json);
        }
    }
});

userCmd.Subcommands.Add(userCreateCmd);

var userDeleteCmd = new Command("delete", "Delete a user");
var userDeleteIdArg = new Argument<Guid>("id");
var userDeleteJsonOpt = NewJsonOption();
userDeleteCmd.Arguments.Add(userDeleteIdArg);
userDeleteCmd.Options.Add(userDeleteJsonOpt);

userDeleteCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(userDeleteJsonOpt);

    var id = result.GetValue(userDeleteIdArg);

    try
    {
        var ok = mediator.Send(new DeleteUserCommand(id)).Result;
        if (!ok)
        {
            WriteError("User not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new { deleted = true, id });
            return;
        }

        AnsiConsole.MarkupLine($"[green]User deleted:[/] {id}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        try
        {
            var ok = store.DeleteAsync<JsonUserEntry>("users", e => e.UserId == id).Result;
            if (!ok)
            {
                WriteError("User not found.", json);
                return;
            }

            if (json)
            {
                WriteJson(new { deleted = true, id });
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - User deleted from local store:[/] {id}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to delete in local store: {storeEx.Message}", json);
        }
    }
});

userCmd.Subcommands.Add(userDeleteCmd);
rootCommand.Subcommands.Add(userCmd);

var systemCmd = new Command("system", "Manage systems");
var systemListCmd = new Command("list", "List systems");
var systemListJsonOpt = NewJsonOption();
systemListCmd.Options.Add(systemListJsonOpt);
systemListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(systemListJsonOpt);

    try
    {
        var systems = references.ListSystemsAsync().Result;
        if (json)
        {
            WriteJson(systems);
            return;
        }

        var table = new Table();
        table.AddColumns("Id", "EAI", "Name", "Owner");
        foreach (var system in systems)
        {
            table.AddRow(system.SystemId.ToString(), system.EAI, system.Name, system.OwnerName);
        }

        AnsiConsole.Write(table);
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entries = store.ReadAllAsync<JsonSystemEntry>("systems").Result;
        if (json)
        {
            WriteJson(entries);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var table = new Table();
        table.AddColumns("Id", "EAI", "Name", "OwnerUserId");
        foreach (var entry in entries)
        {
            table.AddRow(entry.SystemId.ToString(), entry.EAI, entry.Name, entry.OwnerUserId.ToString()[..8]);
        }

        AnsiConsole.Write(table);
    }
});
systemCmd.Subcommands.Add(systemListCmd);

var systemUpdateCmd = new Command("update", "Update a system");
var systemUpdateIdArg = new Argument<long>("id");
var systemUpdateEaiOpt = new Option<string>("--eai") { Required = true };
var systemUpdateNameOpt = new Option<string>("--name") { Required = true };
var systemUpdateOwnerIdOpt = new Option<Guid>("--owner-id");
var systemUpdateOwnerEmailOpt = new Option<string>("--owner-email");
var systemUpdateJsonOpt = NewJsonOption();
systemUpdateCmd.Arguments.Add(systemUpdateIdArg);
systemUpdateCmd.Options.Add(systemUpdateEaiOpt);
systemUpdateCmd.Options.Add(systemUpdateNameOpt);
systemUpdateCmd.Options.Add(systemUpdateOwnerIdOpt);
systemUpdateCmd.Options.Add(systemUpdateOwnerEmailOpt);
systemUpdateCmd.Options.Add(systemUpdateJsonOpt);

systemUpdateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(systemUpdateJsonOpt);

    var id = result.GetValue(systemUpdateIdArg);
    var eai = result.GetValue(systemUpdateEaiOpt)!;
    var name = result.GetValue(systemUpdateNameOpt)!;
    var ownerUserId = ResolveUserId(result.GetValue(systemUpdateOwnerIdOpt), result.GetValue(systemUpdateOwnerEmailOpt), references, json);
    if (ownerUserId is null)
    {
        return;
    }

    try
    {
        var command = new UpdateSystemCommand(id, eai, name, ownerUserId.Value);
        var entry = mediator.Send(command).Result;

        if (entry is null)
        {
            WriteError("System not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new
            {
                entry.SystemId,
                entry.EAI,
                entry.Name,
                entry.OwnerUserId
            });
            return;
        }

        AnsiConsole.MarkupLine($"[green]System updated:[/] {entry.SystemId}");
        AnsiConsole.MarkupLine($"  [bold]EAI:[/] {entry.EAI}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
        AnsiConsole.MarkupLine($"  [bold]Owner:[/] {entry.OwnerUserId}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var existingEntry = store.FindByIdAsync<JsonSystemEntry>("systems", e => e.SystemId == id).Result;
        if (existingEntry is null)
        {
            WriteError("System not found.", json);
            return;
        }

        var updatedEntry = new JsonSystemEntry
        {
            SystemId = id,
            EAI = eai,
            Name = name,
            OwnerUserId = ownerUserId.Value
        };

        try
        {
            store.UpdateAsync("systems", e => e.SystemId == id, updatedEntry).Wait();

            if (json)
            {
                WriteJson(updatedEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - System updated in local store:[/] {id}");
            AnsiConsole.MarkupLine($"  [bold]EAI:[/] {eai}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to update in local store: {storeEx.Message}", json);
        }
    }
});

systemCmd.Subcommands.Add(systemUpdateCmd);

var systemCreateCmd = new Command("create", "Create a new system");
var systemCreateEaiOpt = new Option<string>("--eai") { Required = true };
var systemCreateNameOpt = new Option<string>("--name") { Required = true };
var systemCreateOwnerIdOpt = new Option<Guid>("--owner-id");
var systemCreateOwnerEmailOpt = new Option<string>("--owner-email");
var systemCreateJsonOpt = NewJsonOption();
systemCreateCmd.Options.Add(systemCreateEaiOpt);
systemCreateCmd.Options.Add(systemCreateNameOpt);
systemCreateCmd.Options.Add(systemCreateOwnerIdOpt);
systemCreateCmd.Options.Add(systemCreateOwnerEmailOpt);
systemCreateCmd.Options.Add(systemCreateJsonOpt);

systemCreateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(systemCreateJsonOpt);

    var eai = result.GetValue(systemCreateEaiOpt)!;
    var name = result.GetValue(systemCreateNameOpt)!;
    var ownerUserId = ResolveUserId(result.GetValue(systemCreateOwnerIdOpt), result.GetValue(systemCreateOwnerEmailOpt), references, json);
    if (ownerUserId is null)
        return;

    try
    {
        var entry = mediator.Send(new CreateSystemCommand(eai, name, ownerUserId.Value)).Result;

        if (json)
        {
            WriteJson(new { entry.SystemId, entry.EAI, entry.Name, entry.OwnerUserId });
            return;
        }

        AnsiConsole.MarkupLine($"[green]System created:[/] {entry.SystemId}");
        AnsiConsole.MarkupLine($"  [bold]EAI:[/] {entry.EAI}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
        AnsiConsole.MarkupLine($"  [bold]Owner:[/] {entry.OwnerUserId}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var jsonEntry = new JsonSystemEntry
        {
            SystemId = 0,
            EAI = eai,
            Name = name,
            OwnerUserId = ownerUserId.Value
        };

        try
        {
            store.AppendAsync("systems", jsonEntry).Wait();

            if (json)
            {
                WriteJson(jsonEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - System saved to local store:[/] {eai}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
            AnsiConsole.MarkupLine($"  [bold]Owner:[/] {ownerUserId}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to save to local store: {storeEx.Message}", json);
        }
    }
});

systemCmd.Subcommands.Add(systemCreateCmd);

var systemDeleteCmd = new Command("delete", "Delete a system");
var systemDeleteIdArg = new Argument<long>("id");
var systemDeleteJsonOpt = NewJsonOption();
systemDeleteCmd.Arguments.Add(systemDeleteIdArg);
systemDeleteCmd.Options.Add(systemDeleteJsonOpt);

systemDeleteCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(systemDeleteJsonOpt);

    var id = result.GetValue(systemDeleteIdArg);

    try
    {
        var ok = mediator.Send(new DeleteSystemCommand(id)).Result;
        if (!ok)
        {
            WriteError("System not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new { deleted = true, id });
            return;
        }

        AnsiConsole.MarkupLine($"[green]System deleted:[/] {id}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        try
        {
            var ok = store.DeleteAsync<JsonSystemEntry>("systems", e => e.SystemId == id).Result;
            if (!ok)
            {
                WriteError("System not found.", json);
                return;
            }

            if (json)
            {
                WriteJson(new { deleted = true, id });
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - System deleted from local store:[/] {id}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to delete in local store: {storeEx.Message}", json);
        }
    }
});

systemCmd.Subcommands.Add(systemDeleteCmd);
rootCommand.Subcommands.Add(systemCmd);

var knowledgeTypeCmd = new Command("knowledge-type", "Manage knowledge types");
var knowledgeTypeListCmd = new Command("list", "List knowledge types");
var knowledgeTypeListJsonOpt = NewJsonOption();
knowledgeTypeListCmd.Options.Add(knowledgeTypeListJsonOpt);
knowledgeTypeListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(knowledgeTypeListJsonOpt);

    try
    {
        var types = references.ListKnowledgeTypesAsync().Result;
        WriteReferenceList(types, json, "Id", "Code", "Name");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entries = store.ReadAllAsync<JsonKnowledgeTypeEntry>("knowledge-types").Result;
        if (json)
        {
            WriteJson(entries);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var table = new Table();
        table.AddColumns("Id", "Code", "Name");
        foreach (var entry in entries)
        {
            table.AddRow(entry.TypeId.ToString(), entry.Code, entry.Name);
        }

        AnsiConsole.Write(table);
    }
});
knowledgeTypeCmd.Subcommands.Add(knowledgeTypeListCmd);

var knowledgeTypeUpdateCmd = new Command("update", "Update a knowledge type");
var knowledgeTypeUpdateIdArg = new Argument<long>("id");
var knowledgeTypeUpdateCodeOpt = new Option<string>("--code") { Required = true };
var knowledgeTypeUpdateNameOpt = new Option<string>("--name") { Required = true };
var knowledgeTypeUpdateJsonOpt = NewJsonOption();
knowledgeTypeUpdateCmd.Arguments.Add(knowledgeTypeUpdateIdArg);
knowledgeTypeUpdateCmd.Options.Add(knowledgeTypeUpdateCodeOpt);
knowledgeTypeUpdateCmd.Options.Add(knowledgeTypeUpdateNameOpt);
knowledgeTypeUpdateCmd.Options.Add(knowledgeTypeUpdateJsonOpt);

knowledgeTypeUpdateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeTypeUpdateJsonOpt);

    var id = result.GetValue(knowledgeTypeUpdateIdArg);
    var code = result.GetValue(knowledgeTypeUpdateCodeOpt)!;
    var name = result.GetValue(knowledgeTypeUpdateNameOpt)!;

    try
    {
        var command = new UpdateKnowledgeTypeCommand(id, code, name);
        var entry = mediator.Send(command).Result;

        if (entry is null)
        {
            WriteError("Knowledge type not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new
            {
                entry.TypeId,
                entry.Code,
                entry.Name
            });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge type updated:[/] {entry.TypeId}");
        AnsiConsole.MarkupLine($"  [bold]Code:[/] {entry.Code}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var existingEntry = store.FindByIdAsync<JsonKnowledgeTypeEntry>("knowledge-types", e => e.TypeId == id).Result;
        if (existingEntry is null)
        {
            WriteError("Knowledge type not found.", json);
            return;
        }

        var updatedEntry = new JsonKnowledgeTypeEntry
        {
            TypeId = id,
            Code = code,
            Name = name
        };

        try
        {
            store.UpdateAsync("knowledge-types", e => e.TypeId == id, updatedEntry).Wait();

            if (json)
            {
                WriteJson(updatedEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge type updated in local store:[/] {id}");
            AnsiConsole.MarkupLine($"  [bold]Code:[/] {code}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to update in local store: {storeEx.Message}", json);
        }
    }
});

knowledgeTypeCmd.Subcommands.Add(knowledgeTypeUpdateCmd);

var knowledgeTypeCreateCmd = new Command("create", "Create a new knowledge type");
var knowledgeTypeCreateCodeOpt = new Option<string>("--code") { Required = true };
var knowledgeTypeCreateNameOpt = new Option<string>("--name") { Required = true };
var knowledgeTypeCreateJsonOpt = NewJsonOption();
knowledgeTypeCreateCmd.Options.Add(knowledgeTypeCreateCodeOpt);
knowledgeTypeCreateCmd.Options.Add(knowledgeTypeCreateNameOpt);
knowledgeTypeCreateCmd.Options.Add(knowledgeTypeCreateJsonOpt);

knowledgeTypeCreateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeTypeCreateJsonOpt);

    var code = result.GetValue(knowledgeTypeCreateCodeOpt)!;
    var name = result.GetValue(knowledgeTypeCreateNameOpt)!;

    try
    {
        var entry = mediator.Send(new CreateKnowledgeTypeCommand(code, name)).Result;

        if (json)
        {
            WriteJson(new { entry.TypeId, entry.Code, entry.Name });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge type created:[/] {entry.TypeId}");
        AnsiConsole.MarkupLine($"  [bold]Code:[/] {entry.Code}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var jsonEntry = new JsonKnowledgeTypeEntry
        {
            TypeId = 0,
            Code = code,
            Name = name
        };

        try
        {
            store.AppendAsync("knowledge-types", jsonEntry).Wait();

            if (json)
            {
                WriteJson(jsonEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge type saved to local store:[/] {code}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to save to local store: {storeEx.Message}", json);
        }
    }
});

knowledgeTypeCmd.Subcommands.Add(knowledgeTypeCreateCmd);

var knowledgeTypeDeleteCmd = new Command("delete", "Delete a knowledge type");
var knowledgeTypeDeleteIdArg = new Argument<long>("id");
var knowledgeTypeDeleteJsonOpt = NewJsonOption();
knowledgeTypeDeleteCmd.Arguments.Add(knowledgeTypeDeleteIdArg);
knowledgeTypeDeleteCmd.Options.Add(knowledgeTypeDeleteJsonOpt);

knowledgeTypeDeleteCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeTypeDeleteJsonOpt);

    var id = result.GetValue(knowledgeTypeDeleteIdArg);

    try
    {
        var ok = mediator.Send(new DeleteKnowledgeTypeCommand(id)).Result;
        if (!ok)
        {
            WriteError("Knowledge type not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new { deleted = true, id });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge type deleted:[/] {id}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        try
        {
            var ok = store.DeleteAsync<JsonKnowledgeTypeEntry>("knowledge-types", e => e.TypeId == id).Result;
            if (!ok)
            {
                WriteError("Knowledge type not found.", json);
                return;
            }

            if (json)
            {
                WriteJson(new { deleted = true, id });
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge type deleted from local store:[/] {id}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to delete in local store: {storeEx.Message}", json);
        }
    }
});

knowledgeTypeCmd.Subcommands.Add(knowledgeTypeDeleteCmd);
rootCommand.Subcommands.Add(knowledgeTypeCmd);

var knowledgeStateCmd = new Command("knowledge-state", "Manage knowledge states");
var knowledgeStateListCmd = new Command("list", "List knowledge states");
var knowledgeStateListJsonOpt = NewJsonOption();
knowledgeStateListCmd.Options.Add(knowledgeStateListJsonOpt);
knowledgeStateListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(knowledgeStateListJsonOpt);

    try
    {
        var states = references.ListKnowledgeStatesAsync().Result;
        WriteReferenceList(states, json, "Id", "Code", "Name");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entries = store.ReadAllAsync<JsonKnowledgeStateEntry>("knowledge-states").Result;
        if (json)
        {
            WriteJson(entries);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var table = new Table();
        table.AddColumns("Id", "Code", "Name");
        foreach (var entry in entries)
        {
            table.AddRow(entry.StateId.ToString(), entry.Code, entry.Name);
        }

        AnsiConsole.Write(table);
    }
});
knowledgeStateCmd.Subcommands.Add(knowledgeStateListCmd);

var knowledgeStateUpdateCmd = new Command("update", "Update a knowledge state");
var knowledgeStateUpdateIdArg = new Argument<int>("id");
var knowledgeStateUpdateCodeOpt = new Option<string>("--code") { Required = true };
var knowledgeStateUpdateNameOpt = new Option<string>("--name") { Required = true };
var knowledgeStateUpdateJsonOpt = NewJsonOption();
knowledgeStateUpdateCmd.Arguments.Add(knowledgeStateUpdateIdArg);
knowledgeStateUpdateCmd.Options.Add(knowledgeStateUpdateCodeOpt);
knowledgeStateUpdateCmd.Options.Add(knowledgeStateUpdateNameOpt);
knowledgeStateUpdateCmd.Options.Add(knowledgeStateUpdateJsonOpt);

knowledgeStateUpdateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeStateUpdateJsonOpt);

    var id = result.GetValue(knowledgeStateUpdateIdArg);
    var code = result.GetValue(knowledgeStateUpdateCodeOpt)!;
    var name = result.GetValue(knowledgeStateUpdateNameOpt)!;

    try
    {
        var command = new UpdateKnowledgeStateCommand(id, code, name);
        var entry = mediator.Send(command).Result;

        if (entry is null)
        {
            WriteError("Knowledge state not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new
            {
                entry.StateId,
                entry.Code,
                entry.Name
            });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge state updated:[/] {entry.StateId}");
        AnsiConsole.MarkupLine($"  [bold]Code:[/] {entry.Code}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var existingEntry = store.FindByIdAsync<JsonKnowledgeStateEntry>("knowledge-states", e => e.StateId == id).Result;
        if (existingEntry is null)
        {
            WriteError("Knowledge state not found.", json);
            return;
        }

        var updatedEntry = new JsonKnowledgeStateEntry
        {
            StateId = id,
            Code = code,
            Name = name
        };

        try
        {
            store.UpdateAsync("knowledge-states", e => e.StateId == id, updatedEntry).Wait();

            if (json)
            {
                WriteJson(updatedEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge state updated in local store:[/] {id}");
            AnsiConsole.MarkupLine($"  [bold]Code:[/] {code}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to update in local store: {storeEx.Message}", json);
        }
    }
});

knowledgeStateCmd.Subcommands.Add(knowledgeStateUpdateCmd);

var knowledgeStateCreateCmd = new Command("create", "Create a new knowledge state");
var knowledgeStateCreateCodeOpt = new Option<string>("--code") { Required = true };
var knowledgeStateCreateNameOpt = new Option<string>("--name") { Required = true };
var knowledgeStateCreateJsonOpt = NewJsonOption();
knowledgeStateCreateCmd.Options.Add(knowledgeStateCreateCodeOpt);
knowledgeStateCreateCmd.Options.Add(knowledgeStateCreateNameOpt);
knowledgeStateCreateCmd.Options.Add(knowledgeStateCreateJsonOpt);

knowledgeStateCreateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeStateCreateJsonOpt);

    var code = result.GetValue(knowledgeStateCreateCodeOpt)!;
    var name = result.GetValue(knowledgeStateCreateNameOpt)!;

    try
    {
        var entry = mediator.Send(new CreateKnowledgeStateCommand(code, name)).Result;

        if (json)
        {
            WriteJson(new { entry.StateId, entry.Code, entry.Name });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge state created:[/] {entry.StateId}");
        AnsiConsole.MarkupLine($"  [bold]Code:[/] {entry.Code}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var jsonEntry = new JsonKnowledgeStateEntry
        {
            StateId = 0,
            Code = code,
            Name = name
        };

        try
        {
            store.AppendAsync("knowledge-states", jsonEntry).Wait();

            if (json)
            {
                WriteJson(jsonEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge state saved to local store:[/] {code}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to save to local store: {storeEx.Message}", json);
        }
    }
});

knowledgeStateCmd.Subcommands.Add(knowledgeStateCreateCmd);

var knowledgeStateDeleteCmd = new Command("delete", "Delete a knowledge state");
var knowledgeStateDeleteIdArg = new Argument<int>("id");
var knowledgeStateDeleteJsonOpt = NewJsonOption();
knowledgeStateDeleteCmd.Arguments.Add(knowledgeStateDeleteIdArg);
knowledgeStateDeleteCmd.Options.Add(knowledgeStateDeleteJsonOpt);

knowledgeStateDeleteCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeStateDeleteJsonOpt);

    var id = result.GetValue(knowledgeStateDeleteIdArg);

    try
    {
        var ok = mediator.Send(new DeleteKnowledgeStateCommand(id)).Result;
        if (!ok)
        {
            WriteError("Knowledge state not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new { deleted = true, id });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge state deleted:[/] {id}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        try
        {
            var ok = store.DeleteAsync<JsonKnowledgeStateEntry>("knowledge-states", e => e.StateId == id).Result;
            if (!ok)
            {
                WriteError("Knowledge state not found.", json);
                return;
            }

            if (json)
            {
                WriteJson(new { deleted = true, id });
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge state deleted from local store:[/] {id}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to delete in local store: {storeEx.Message}", json);
        }
    }
});

knowledgeStateCmd.Subcommands.Add(knowledgeStateDeleteCmd);
rootCommand.Subcommands.Add(knowledgeStateCmd);

var issueStateCmd = new Command("issue-state", "Manage issue states");
var issueStateListCmd = new Command("list", "List issue states");
var issueStateListJsonOpt = NewJsonOption();
issueStateListCmd.Options.Add(issueStateListJsonOpt);
issueStateListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var references = scope.ServiceProvider.GetRequiredService<IReferenceDataService>();
    var json = result.GetValue(issueStateListJsonOpt);

    try
    {
        var states = references.ListIssueStatesAsync().Result;
        WriteReferenceList(states, json, "Id", "Code", "Name");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entries = store.ReadAllAsync<JsonIssueStateEntry>("issue-states").Result;
        if (json)
        {
            WriteJson(entries);
            return;
        }

        AnsiConsole.MarkupLine("[yellow]DB unavailable - showing data from local store[/]");
        var table = new Table();
        table.AddColumns("Id", "Code", "Name");
        foreach (var entry in entries)
        {
            table.AddRow(entry.StateId.ToString(), entry.Code, entry.Name);
        }

        AnsiConsole.Write(table);
    }
});
issueStateCmd.Subcommands.Add(issueStateListCmd);

var issueStateUpdateCmd = new Command("update", "Update an issue state");
var issueStateUpdateIdArg = new Argument<int>("id");
var issueStateUpdateCodeOpt = new Option<string>("--code") { Required = true };
var issueStateUpdateNameOpt = new Option<string>("--name") { Required = true };
var issueStateUpdateJsonOpt = NewJsonOption();
issueStateUpdateCmd.Arguments.Add(issueStateUpdateIdArg);
issueStateUpdateCmd.Options.Add(issueStateUpdateCodeOpt);
issueStateUpdateCmd.Options.Add(issueStateUpdateNameOpt);
issueStateUpdateCmd.Options.Add(issueStateUpdateJsonOpt);

issueStateUpdateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(issueStateUpdateJsonOpt);

    var id = result.GetValue(issueStateUpdateIdArg);
    var code = result.GetValue(issueStateUpdateCodeOpt)!;
    var name = result.GetValue(issueStateUpdateNameOpt)!;

    try
    {
        var command = new UpdateIssueStateCommand(id, code, name);
        var entry = mediator.Send(command).Result;

        if (entry is null)
        {
            WriteError("Issue state not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new
            {
                entry.StateId,
                entry.Code,
                entry.Name
            });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Issue state updated:[/] {entry.StateId}");
        AnsiConsole.MarkupLine($"  [bold]Code:[/] {entry.Code}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var existingEntry = store.FindByIdAsync<JsonIssueStateEntry>("issue-states", e => e.StateId == id).Result;
        if (existingEntry is null)
        {
            WriteError("Issue state not found.", json);
            return;
        }

        var updatedEntry = new JsonIssueStateEntry
        {
            StateId = id,
            Code = code,
            Name = name
        };

        try
        {
            store.UpdateAsync("issue-states", e => e.StateId == id, updatedEntry).Wait();

            if (json)
            {
                WriteJson(updatedEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue state updated in local store:[/] {id}");
            AnsiConsole.MarkupLine($"  [bold]Code:[/] {code}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to update in local store: {storeEx.Message}", json);
        }
    }
});

issueStateCmd.Subcommands.Add(issueStateUpdateCmd);

var issueStateCreateCmd = new Command("create", "Create a new issue state");
var issueStateCreateCodeOpt = new Option<string>("--code") { Required = true };
var issueStateCreateNameOpt = new Option<string>("--name") { Required = true };
var issueStateCreateJsonOpt = NewJsonOption();
issueStateCreateCmd.Options.Add(issueStateCreateCodeOpt);
issueStateCreateCmd.Options.Add(issueStateCreateNameOpt);
issueStateCreateCmd.Options.Add(issueStateCreateJsonOpt);

issueStateCreateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(issueStateCreateJsonOpt);

    var code = result.GetValue(issueStateCreateCodeOpt)!;
    var name = result.GetValue(issueStateCreateNameOpt)!;

    try
    {
        var entry = mediator.Send(new CreateIssueStateCommand(code, name)).Result;

        if (json)
        {
            WriteJson(new { entry.StateId, entry.Code, entry.Name });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Issue state created:[/] {entry.StateId}");
        AnsiConsole.MarkupLine($"  [bold]Code:[/] {entry.Code}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.Name}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var jsonEntry = new JsonIssueStateEntry
        {
            StateId = 0,
            Code = code,
            Name = name
        };

        try
        {
            store.AppendAsync("issue-states", jsonEntry).Wait();

            if (json)
            {
                WriteJson(jsonEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue state saved to local store:[/] {code}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to save to local store: {storeEx.Message}", json);
        }
    }
});

issueStateCmd.Subcommands.Add(issueStateCreateCmd);

var issueStateDeleteCmd = new Command("delete", "Delete an issue state");
var issueStateDeleteIdArg = new Argument<int>("id");
var issueStateDeleteJsonOpt = NewJsonOption();
issueStateDeleteCmd.Arguments.Add(issueStateDeleteIdArg);
issueStateDeleteCmd.Options.Add(issueStateDeleteJsonOpt);

issueStateDeleteCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(issueStateDeleteJsonOpt);

    var id = result.GetValue(issueStateDeleteIdArg);

    try
    {
        var ok = mediator.Send(new DeleteIssueStateCommand(id)).Result;
        if (!ok)
        {
            WriteError("Issue state not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new { deleted = true, id });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Issue state deleted:[/] {id}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        try
        {
            var ok = store.DeleteAsync<JsonIssueStateEntry>("issue-states", e => e.StateId == id).Result;
            if (!ok)
            {
                WriteError("Issue state not found.", json);
                return;
            }

            if (json)
            {
                WriteJson(new { deleted = true, id });
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Issue state deleted from local store:[/] {id}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to delete in local store: {storeEx.Message}", json);
        }
    }
});

issueStateCmd.Subcommands.Add(issueStateDeleteCmd);
rootCommand.Subcommands.Add(issueStateCmd);

var knowledgeTagCmd = new Command("knowledge-tag", "Manage knowledge tags");

var knowledgeTagListCmd = new Command("list", "List all knowledge tags");
var knowledgeTagListJsonOpt = NewJsonOption();
knowledgeTagListCmd.Options.Add(knowledgeTagListJsonOpt);
knowledgeTagListCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var tagRepo = scope.ServiceProvider.GetRequiredService<IKnowledgeTagRepository>();
    var json = result.GetValue(knowledgeTagListJsonOpt);

    try
    {
        var tags = tagRepo.GetAllAsync().Result;
        if (json)
        {
            WriteJson(tags);
            return;
        }

        var table = new Table();
        table.AddColumns("Id", "Name");
        foreach (var tag in tags)
        {
            table.AddRow(tag.KnowledgeTagId.ToString(), tag.TagName);
        }

        AnsiConsole.Write(table);
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available.", json);
            return;
        }

        var entries = store.ReadAllAsync<JsonKnowledgeTagEntry>("knowledge-tags").Result;
        if (json)
        {
            WriteJson(entries);
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
knowledgeTagCmd.Subcommands.Add(knowledgeTagListCmd);

var knowledgeTagUpdateCmd = new Command("update", "Update a knowledge tag");
var knowledgeTagUpdateIdArg = new Argument<long>("id");
var knowledgeTagUpdateNameOpt = new Option<string>("--name") { Required = true };
var knowledgeTagUpdateJsonOpt = NewJsonOption();
knowledgeTagUpdateCmd.Arguments.Add(knowledgeTagUpdateIdArg);
knowledgeTagUpdateCmd.Options.Add(knowledgeTagUpdateNameOpt);
knowledgeTagUpdateCmd.Options.Add(knowledgeTagUpdateJsonOpt);

knowledgeTagUpdateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeTagUpdateJsonOpt);

    var id = result.GetValue(knowledgeTagUpdateIdArg);
    var tagName = result.GetValue(knowledgeTagUpdateNameOpt)!;

    try
    {
        var command = new UpdateKnowledgeTagCommand(id, tagName);
        var entry = mediator.Send(command).Result;

        if (entry is null)
        {
            WriteError("Knowledge tag not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new
            {
                entry.KnowledgeTagId,
                entry.TagName
            });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge tag updated:[/] {entry.KnowledgeTagId}");
        AnsiConsole.MarkupLine($"  [bold]Name:[/] {entry.TagName}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var existingEntry = store.FindByIdAsync<JsonKnowledgeTagEntry>("knowledge-tags", e => e.KnowledgeTagId == id).Result;
        if (existingEntry is null)
        {
            WriteError("Knowledge tag not found.", json);
            return;
        }

        var updatedEntry = new JsonKnowledgeTagEntry
        {
            KnowledgeTagId = id,
            TagName = tagName
        };

        try
        {
            store.UpdateAsync("knowledge-tags", e => e.KnowledgeTagId == id, updatedEntry).Wait();

            if (json)
            {
                WriteJson(updatedEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge tag updated in local store:[/] {id}");
            AnsiConsole.MarkupLine($"  [bold]Name:[/] {tagName}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to update in local store: {storeEx.Message}", json);
        }
    }
});

knowledgeTagCmd.Subcommands.Add(knowledgeTagUpdateCmd);

var knowledgeTagCreateCmd = new Command("create", "Create a new knowledge tag");
var knowledgeTagCreateNameOpt = new Option<string>("--name") { Required = true };
var knowledgeTagCreateJsonOpt = NewJsonOption();
knowledgeTagCreateCmd.Options.Add(knowledgeTagCreateNameOpt);
knowledgeTagCreateCmd.Options.Add(knowledgeTagCreateJsonOpt);

knowledgeTagCreateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeTagCreateJsonOpt);

    var name = result.GetValue(knowledgeTagCreateNameOpt)!;

    try
    {
        var entry = mediator.Send(new CreateKnowledgeTagCommand(name)).Result;

        if (json)
        {
            WriteJson(new { entry.KnowledgeTagId, entry.TagName });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge tag created:[/] {entry.KnowledgeTagId}");
        AnsiConsole.MarkupLine($"  [bold]Tag:[/] {entry.TagName}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be created.", json);
            return;
        }

        var jsonEntry = new JsonKnowledgeTagEntry
        {
            KnowledgeTagId = 0,
            TagName = name
        };

        try
        {
            store.AppendAsync("knowledge-tags", jsonEntry).Wait();

            if (json)
            {
                WriteJson(jsonEntry);
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge tag saved to local store:[/] {name}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to save to local store: {storeEx.Message}", json);
        }
    }
});

knowledgeTagCmd.Subcommands.Add(knowledgeTagCreateCmd);

var knowledgeTagDeleteCmd = new Command("delete", "Delete a knowledge tag");
var knowledgeTagDeleteIdArg = new Argument<long>("id");
var knowledgeTagDeleteJsonOpt = NewJsonOption();
knowledgeTagDeleteCmd.Arguments.Add(knowledgeTagDeleteIdArg);
knowledgeTagDeleteCmd.Options.Add(knowledgeTagDeleteJsonOpt);

knowledgeTagDeleteCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var json = result.GetValue(knowledgeTagDeleteJsonOpt);

    var id = result.GetValue(knowledgeTagDeleteIdArg);

    try
    {
        var ok = mediator.Send(new DeleteKnowledgeTagCommand(id)).Result;
        if (!ok)
        {
            WriteError("Knowledge tag not found.", json);
            return;
        }

        if (json)
        {
            WriteJson(new { deleted = true, id });
            return;
        }

        AnsiConsole.MarkupLine($"[green]Knowledge tag deleted:[/] {id}");
    }
    catch
    {
        var store = CreateJsonStore();
        if (store is null)
        {
            WriteError("Database is not available and JSON store could not be obtained.", json);
            return;
        }

        try
        {
            var ok = store.DeleteAsync<JsonKnowledgeTagEntry>("knowledge-tags", e => e.KnowledgeTagId == id).Result;
            if (!ok)
            {
                WriteError("Knowledge tag not found.", json);
                return;
            }

            if (json)
            {
                WriteJson(new { deleted = true, id });
                return;
            }

            AnsiConsole.MarkupLine($"[yellow]DB unavailable - Knowledge tag deleted from local store:[/] {id}");
        }
        catch (Exception storeEx)
        {
            WriteError($"Failed to delete in local store: {storeEx.Message}", json);
        }
    }
});

knowledgeTagCmd.Subcommands.Add(knowledgeTagDeleteCmd);
rootCommand.Subcommands.Add(knowledgeTagCmd);

var startupCmd = new Command("startup", "Initialize reference data (Users, Systems, KnowledgeTypes, IssueStates, KnowledgeStates)");
var startupDemoOpt = new Option<bool>("--demo")
{
    Description = "Seed idempotent demo data without prompts"
};
var startupJsonOpt = NewJsonOption();
startupCmd.Options.Add(startupDemoOpt);
startupCmd.Options.Add(startupJsonOpt);

startupCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var svc = scope.ServiceProvider.GetRequiredService<IStartupService>();
    var ct = CancellationToken.None;
    var json = result.GetValue(startupJsonOpt);

    if (result.GetValue(startupDemoOpt))
    {
        if (json)
        {
            WriteJson(svc.SeedDemoDataAsync(ct).Result);
            return;
        }

        AnsiConsole.Status()
            .Start("Seeding demo data...", _ =>
            {
                svc.SeedDemoDataAsync(ct).Wait();
            });
        AnsiConsole.MarkupLine("[bold green]Demo data initialized successfully![/]");
        return;
    }

    if (json)
    {
        WriteError("Use --json together with --demo for non-interactive startup output.", true);
        return;
    }

    AnsiConsole.Write(new FigletText("Axiom Setup").Color(Color.Yellow));
    AnsiConsole.Write(new Rule("[yellow]Reference Data Initialization[/]"));
    AnsiConsole.MarkupLine("This wizard will help you load the initial reference data.\n");

    var users = new List<User>();
    AnsiConsole.Write(new Rule("[cyan]Users[/]"));
    do
    {
        var email = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Email:[/]"));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]"));
        users.Add(new User(email, name));
    }
    while (AnsiConsole.Confirm("Add another user?", false));

    var systems = new List<AxiomSystem>();
    AnsiConsole.Write(new Rule("[cyan]Systems[/]"));
    do
    {
        var eai = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]EAI:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]EAI cannot be empty[/]") : ValidationResult.Success()));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Name cannot be empty[/]") : ValidationResult.Success()));
        var owner = AnsiConsole.Prompt(
            new SelectionPrompt<User>()
                .Title("[cyan]Owner:[/]")
                .AddChoices(users)
                .UseConverter(u => $"{u.Name} ({u.Email})"));
        systems.Add(new AxiomSystem(eai, name, owner.UserId));
    }
    while (AnsiConsole.Confirm("Add another system?", false));

    var knowledgeTypes = new List<KnowledgeType>();
    AnsiConsole.Write(new Rule("[cyan]Knowledge Types[/]"));
    do
    {
        var code = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Code:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Code cannot be empty[/]") : ValidationResult.Success()));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Name cannot be empty[/]") : ValidationResult.Success()));
        knowledgeTypes.Add(new KnowledgeType(code, name));
    }
    while (AnsiConsole.Confirm("Add another type?", false));

    var issueStates = new List<IssueState>();
    AnsiConsole.Write(new Rule("[cyan]Issue States[/]"));
    do
    {
        var code = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Code:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Code cannot be empty[/]") : ValidationResult.Success()));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Name cannot be empty[/]") : ValidationResult.Success()));
        issueStates.Add(new IssueState(code, name));
    }
    while (AnsiConsole.Confirm("Add another state?", false));

    var knowledgeStates = new List<KnowledgeState>();
    AnsiConsole.Write(new Rule("[cyan]Knowledge States[/]"));
    do
    {
        var code = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Code:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Code cannot be empty[/]") : ValidationResult.Success()));
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[cyan]Name:[/]").Validate(v =>
            string.IsNullOrWhiteSpace(v) ? ValidationResult.Error("[red]Name cannot be empty[/]") : ValidationResult.Success()));
        knowledgeStates.Add(new KnowledgeState(code, name));
    }
    while (AnsiConsole.Confirm("Add another state?", false));

    AnsiConsole.Write(new Rule("[yellow]Summary[/]"));
    var summaryTable = new Table();
    summaryTable.AddColumns("Entity", "Count");
    summaryTable.AddRow("Users", users.Count.ToString());
    summaryTable.AddRow("Systems", systems.Count.ToString());
    summaryTable.AddRow("Knowledge Types", knowledgeTypes.Count.ToString());
    summaryTable.AddRow("Issue States", issueStates.Count.ToString());
    summaryTable.AddRow("Knowledge States", knowledgeStates.Count.ToString());
    AnsiConsole.Write(summaryTable);

    if (!AnsiConsole.Confirm("Proceed with seeding?", true))
    {
        AnsiConsole.MarkupLine("[red]Operation cancelled.[/]");
        return;
    }

    var totalUsers = 0;
    var totalSystems = 0;
    var totalTypes = 0;
    var totalIssueStates = 0;
    var totalKnowledgeStates = 0;

    AnsiConsole.Status()
        .Start("Seeding reference data...", ctx =>
        {
            ctx.Spinner(Spinner.Known.Star);

            var persistedUserIds = new Dictionary<Guid, Guid>();

            foreach (var u in users)
            {
                var persistedUser = svc.CreateUserAsync(u.Email, u.Name, ct).Result;
                persistedUserIds[u.UserId] = persistedUser.UserId;
                totalUsers++;
                ctx.Status($"Created user: {u.Email}");
            }

            foreach (var s in systems)
            {
                var ownerUserId = persistedUserIds[s.OwnerUserId];
                svc.CreateSystemAsync(s.EAI, s.Name, ownerUserId, ct).Wait();
                totalSystems++;
                ctx.Status($"Created system: {s.Name}");
            }

            foreach (var t in knowledgeTypes)
            {
                svc.CreateKnowledgeTypeAsync(t.Code, t.Name, ct).Wait();
                totalTypes++;
                ctx.Status($"Created type: {t.Code}");
            }

            foreach (var s in issueStates)
            {
                svc.CreateIssueStateAsync(s.Code, s.Name, ct).Wait();
                totalIssueStates++;
                ctx.Status($"Created issue state: {s.Code}");
            }

            foreach (var s in knowledgeStates)
            {
                svc.CreateKnowledgeStateAsync(s.Code, s.Name, ct).Wait();
                totalKnowledgeStates++;
                ctx.Status($"Created knowledge state: {s.Code}");
            }
        });

    var resultTable = new Table();
    resultTable.AddColumns("Entity", "Created");
    resultTable.AddRow("[green]Users[/]", totalUsers.ToString());
    resultTable.AddRow("[green]Systems[/]", totalSystems.ToString());
    resultTable.AddRow("[green]Knowledge Types[/]", totalTypes.ToString());
    resultTable.AddRow("[green]Issue States[/]", totalIssueStates.ToString());
    resultTable.AddRow("[green]Knowledge States[/]", totalKnowledgeStates.ToString());
    AnsiConsole.Write(resultTable);
    AnsiConsole.MarkupLine("[bold green]Reference data initialized successfully![/]");
});

rootCommand.Subcommands.Add(startupCmd);

void WriteJson(object value)
{
    Console.WriteLine(JsonSerializer.Serialize(value, jsonOptions));
}

void WriteError(string message, bool json)
{
    Environment.ExitCode = 1;
    if (json)
    {
        WriteJson(new { error = message });
        return;
    }

    AnsiConsole.MarkupLine($"[red]{message}[/]");
}

JsonStore? CreateJsonStore()
{
    try
    {
        return new JsonStore();
    }
    catch
    {
        return null;
    }
}

void WriteReferenceList(IEnumerable<Axiom.Application.Dtos.ReferenceCodeDto> references, bool json, params string[] columns)
{
    if (json)
    {
        WriteJson(references);
        return;
    }

    var table = new Table();
    table.AddColumns(columns);
    foreach (var reference in references)
    {
        table.AddRow(reference.Id.ToString(), reference.Code, reference.Name);
    }

    AnsiConsole.Write(table);
}

long? ResolveSystemId(long id, string? eai, IReferenceDataService references, bool json)
{
    if (id > 0 && !string.IsNullOrWhiteSpace(eai))
    {
        WriteError("Use either --system-id or --system-eai, not both.", json);
        return null;
    }

    if (id > 0)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(eai))
    {
        WriteError("A system reference is required. Use --system-id or --system-eai.", json);
        return null;
    }

    var system = references.FindSystemByEaiAsync(eai).Result;
    if (system is null)
    {
        WriteError($"System EAI not found: {eai}", json);
        return null;
    }

    return system.SystemId;
}

long? ResolveKnowledgeTypeId(long id, string? code, IReferenceDataService references, bool json)
{
    if (id > 0 && !string.IsNullOrWhiteSpace(code))
    {
        WriteError("Use either --type-id or --type-code, not both.", json);
        return null;
    }

    if (id > 0)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(code))
    {
        WriteError("A knowledge type reference is required. Use --type-id or --type-code.", json);
        return null;
    }

    var type = references.FindKnowledgeTypeByCodeAsync(code).Result;
    if (type is null)
    {
        WriteError($"Knowledge type code not found: {code}", json);
        return null;
    }

    return type.Id;
}

int? ResolveKnowledgeStateId(int id, string? code, IReferenceDataService references, bool json)
{
    if (id > 0 && !string.IsNullOrWhiteSpace(code))
    {
        WriteError("Use either --state-id or --state-code, not both.", json);
        return null;
    }

    if (id > 0)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(code))
    {
        WriteError("A knowledge state reference is required. Use --state-id or --state-code.", json);
        return null;
    }

    var state = references.FindKnowledgeStateByCodeAsync(code).Result;
    if (state is null)
    {
        WriteError($"Knowledge state code not found: {code}", json);
        return null;
    }

    return (int)state.Id;
}

int? ResolveIssueStateId(int id, string? code, IReferenceDataService references, bool json)
{
    if (id > 0 && !string.IsNullOrWhiteSpace(code))
    {
        WriteError("Use either --state-id or --state-code, not both.", json);
        return null;
    }

    if (id > 0)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(code))
    {
        WriteError("An issue state reference is required. Use --state-id or --state-code.", json);
        return null;
    }

    var state = references.FindIssueStateByCodeAsync(code).Result;
    if (state is null)
    {
        WriteError($"Issue state code not found: {code}", json);
        return null;
    }

    return (int)state.Id;
}

Guid? ResolveUserId(Guid id, string? email, IReferenceDataService references, bool json)
{
    if (id != Guid.Empty && !string.IsNullOrWhiteSpace(email))
    {
        WriteError("Use either --created-by or --created-by-email, not both.", json);
        return null;
    }

    if (id != Guid.Empty)
    {
        return id;
    }

    if (string.IsNullOrWhiteSpace(email))
    {
        WriteError("A user reference is required. Use --created-by or --created-by-email.", json);
        return null;
    }

    var user = references.FindUserByEmailAsync(email).Result;
    if (user is null)
    {
        WriteError($"User email not found: {email}", json);
        return null;
    }

    return user.UserId;
}

object ToKnowledgeCreateResult(Knowledge entry)
{
    return new
    {
        entry.KnowledgeId,
        entry.Title,
        entry.Summary,
        entry.Content,
        entry.SystemId,
        entry.CreatedByUserId,
        entry.KnowledgeTypeId,
        entry.KnowledgeStateId,
        entry.IssueId,
        entry.VersionNumber,
        entry.CreatedAt,
        entry.UpdatedAt
    };
}

object ToKnowledgeDetails(Knowledge entry)
{
    return new
    {
        entry.KnowledgeId,
        entry.Title,
        entry.Summary,
        entry.Content,
        entry.SystemId,
        SystemName = entry.System?.Name,
        entry.CreatedByUserId,
        CreatedByName = entry.CreatedBy?.Name,
        entry.KnowledgeTypeId,
        TypeName = entry.Type?.Name,
        entry.KnowledgeStateId,
        StateName = entry.State?.Name,
        entry.IssueId,
        Tags = entry.KnowledgeKnowledgeTags
            .Select(t => t.Tag?.TagName)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList(),
        entry.VersionNumber,
        entry.CreatedAt,
        entry.UpdatedAt
    };
}

object ToIssueCreateResult(Issue issue)
{
    return new
    {
        issue.IssueId,
        issue.Summary,
        issue.SystemId,
        issue.StateId,
        issue.CreatedByUserId,
        issue.RitmNumber,
        issue.IncidentNumber,
        issue.CreatedAt,
        issue.UpdatedAt,
        issue.ResolvedAt
    };
}

object ToIssueDetails(Issue issue)
{
    return new
    {
        issue.IssueId,
        issue.Summary,
        issue.SystemId,
        SystemName = issue.System?.Name,
        issue.StateId,
        StateName = issue.State?.Name,
        issue.CreatedByUserId,
        CreatedByName = issue.CreatedBy?.Name,
        issue.RitmNumber,
        issue.IncidentNumber,
        issue.Problem,
        issue.Analysis,
        issue.Resolution,
        issue.CreatedAt,
        issue.UpdatedAt,
        issue.ResolvedAt
    };
}

var parseResult = rootCommand.Parse(args);
var exitCode = await parseResult.InvokeAsync();
return Environment.ExitCode != 0 ? Environment.ExitCode : exitCode;
