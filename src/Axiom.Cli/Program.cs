using System.CommandLine;
using System.CommandLine.Parsing;
using Axiom.Application;
using Axiom.Application.Commands;
using Axiom.Application.Queries;
using Axiom.Domain.Enums;
using Axiom.Domain.ValueObjects;
using Axiom.Infrastructure;
using Axiom.Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

/// <summary>
/// Axiom CLI — KnowledgeOps and Operational Continuity Platform.
/// Provides commands for managing knowledge entries and case records through a command-line interface.
/// </summary>
var builder = Host.CreateApplicationBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("AXIOM_CONNECTION_STRING")
    ?? "Server=localhost;Database=AXIOM;Integrated Security=True;TrustServerCertificate=True;";

builder.Services
    .AddApplication()
    .AddInfrastructureEF(connectionString)
    .Configure<JsonDataOptions>(builder.Configuration.GetSection(JsonDataOptions.SectionName));

var host = builder.Build();

/// <summary>
/// Root command for the Axiom application. All subcommands are registered under this root.
/// </summary>
var rootCommand = new RootCommand("Axiom - KnowledgeOps and Operational Continuity Platform");

/// <summary>
/// Defines the "knowledge" command group and its subcommands (create, list, show, search).
/// </summary>
var knowledgeCmd = new Command("knowledge", "Manage knowledge entries");

/// <summary>
/// Subcommand: Creates a new knowledge entry with the provided metadata and content.
/// </summary>
var createCmd = new Command("create", "Create a new knowledge entry");
/// <summary>The title of the knowledge entry (required).</summary>
var titleOpt = new Option<string>("--title") { Required = true };
/// <summary>An optional description of the knowledge entry.</summary>
var descOpt = new Option<string>("--description");
/// <summary>The main body content of the knowledge entry (required).</summary>
var contentOpt = new Option<string>("--content") { Required = true };
/// <summary>The associated system or application name (required).</summary>
var systemOpt = new Option<string>("--system") { Required = true };
/// <summary>Comma-separated tags for categorization.</summary>
var tagsOpt = new Option<string>("--tags");
/// <summary>The author of the knowledge entry (required).</summary>
var authorOpt = new Option<string>("--author") { Required = true };
/// <summary>The content type. Allowed values: Documentation, Runbook, Troubleshooting, Reference, Tutorial, Other.</summary>
var typeOpt = new Option<KnowledgeType>("--type") { Arity = new ArgumentArity(0, 1) };
createCmd.Options.Add(titleOpt);
createCmd.Options.Add(descOpt);
createCmd.Options.Add(contentOpt);
createCmd.Options.Add(systemOpt);
createCmd.Options.Add(tagsOpt);
createCmd.Options.Add(authorOpt);
createCmd.Options.Add(typeOpt);

createCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var tags = result.GetValue(tagsOpt);
    var tagList = string.IsNullOrWhiteSpace(tags) ? new List<string>() : [.. tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

    var command = new CreateKnowledgeCommand(
        result.GetValue(titleOpt)!,
        result.GetValue(descOpt) ?? string.Empty,
        result.GetValue(contentOpt)!,
        result.GetValue(systemOpt)!,
        tagList,
        result.GetValue(authorOpt)!,
        result.GetValue(typeOpt),
        KnowledgeStatusValue.Draft);

    var entry = mediator.Send(command).Result;

    AnsiConsole.MarkupLine($"[green]Knowledge entry created:[/] {entry.Id}");
    AnsiConsole.MarkupLine($"  [bold]Title:[/] {entry.Title}");
    AnsiConsole.MarkupLine($"  [bold]System:[/] {entry.System}");
    AnsiConsole.MarkupLine($"  [bold]Type:[/] {entry.Type}");
    AnsiConsole.MarkupLine($"  [bold]Status:[/] {entry.Status}");
});

knowledgeCmd.Subcommands.Add(createCmd);

/// <summary>
/// Subcommand: Lists all knowledge entries in a table format.
/// </summary>
var listCmd = new Command("list", "List all knowledge entries");
listCmd.SetAction((ParseResult _) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var entries = mediator.Send(new ListKnowledgeQuery()).Result;

    var table = new Table();
    table.AddColumns("Id", "Title", "System", "Type", "Status", "Updated");

    foreach (var entry in entries)
    {
        table.AddRow(
            entry.Id.ToString()[..8],
            entry.Title,
            entry.System.ToString(),
            entry.Type.ToString(),
            entry.Status.ToString(),
            entry.UpdatedAt.ToString("yyyy-MM-dd"));
    }

    AnsiConsole.Write(table);
});

knowledgeCmd.Subcommands.Add(listCmd);

/// <summary>
/// Subcommand: Displays detailed information for a single knowledge entry by its identifier.
/// </summary>
var showCmd = new Command("show", "Show knowledge entry details");
/// <summary>The unique identifier (GUID) of the knowledge entry to display.</summary>
var idArg = new Argument<Guid>("id");
showCmd.Arguments.Add(idArg);
showCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var entry = mediator.Send(new GetKnowledgeByIdQuery(result.GetValue(idArg))).Result;

    if (entry is null)
    {
        AnsiConsole.MarkupLine("[red]Knowledge entry not found.[/]");
        return;
    }

    var panel = new Panel(
        new Markup(
            $"[bold]Title:[/] {entry.Title}\n" +
            $"[bold]Description:[/] {entry.Description}\n" +
            $"[bold]System:[/] {entry.System}\n" +
            $"[bold]Type:[/] {entry.Type}\n" +
            $"[bold]Status:[/] {entry.Status}\n" +
            $"[bold]Author:[/] {entry.Author}\n" +
            $"[bold]Tags:[/] {string.Join(", ", entry.Tags)}\n" +
            $"[bold]Created:[/] {entry.CreatedAt:yyyy-MM-dd HH:mm:ss}\n" +
            $"[bold]Updated:[/] {entry.UpdatedAt:yyyy-MM-dd HH:mm:ss}\n" +
            $"[bold]Content:[/]\n{entry.Content}"))
    {
        Header = new PanelHeader($"Knowledge Entry - {entry.Id}")
    };

    AnsiConsole.Write(panel);
});

knowledgeCmd.Subcommands.Add(showCmd);

/// <summary>
/// Subcommand: Searches knowledge entries by matching text against title, description, content, and tags.
/// </summary>
var searchCmd = new Command("search", "Search knowledge entries");
/// <summary>The search text used to find matching knowledge entries.</summary>
var queryArg = new Argument<string>("query");
searchCmd.Arguments.Add(queryArg);
searchCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var query = result.GetValue(queryArg) ?? string.Empty;
    var entries = mediator.Send(new SearchKnowledgeQuery(query)).Result;

    var table = new Table();
    table.AddColumns("Id", "Title", "System", "Type", "Status");

    foreach (var entry in entries)
    {
        table.AddRow(
            entry.Id.ToString()[..8],
            entry.Title,
            entry.System.ToString(),
            entry.Type.ToString(),
            entry.Status.ToString());
    }

    AnsiConsole.Write(table);
});

knowledgeCmd.Subcommands.Add(searchCmd);
rootCommand.Subcommands.Add(knowledgeCmd);

/// <summary>
/// Defines the "case" command group and its subcommands (create, show).
/// </summary>
var caseCmd = new Command("case", "Manage case records");

/// <summary>
/// Subcommand: Creates a new case record with the specified details.
/// </summary>
var caseCreateCmd = new Command("create", "Create a new case record");
/// <summary>The associated system or application name (required).</summary>
var caseSystemOpt = new Option<string>("--system") { Required = true };
/// <summary>A description of the problem or issue (required).</summary>
var problemOpt = new Option<string>("--problem") { Required = true };
/// <summary>Root cause analysis notes.</summary>
var analysisOpt = new Option<string>("--analysis");
/// <summary>Resolution steps applied.</summary>
var resolutionOpt = new Option<string>("--resolution");
/// <summary>Lessons learned from handling the case.</summary>
var lessonsOpt = new Option<string>("--lessons");
/// <summary>An optional RITM (Request Item) identifier.</summary>
var ritmOpt = new Option<string>("--ritm-id");
/// <summary>An optional change request identifier.</summary>
var changeOpt = new Option<string>("--change-id");
caseCreateCmd.Options.Add(caseSystemOpt);
caseCreateCmd.Options.Add(problemOpt);
caseCreateCmd.Options.Add(analysisOpt);
caseCreateCmd.Options.Add(resolutionOpt);
caseCreateCmd.Options.Add(lessonsOpt);
caseCreateCmd.Options.Add(ritmOpt);
caseCreateCmd.Options.Add(changeOpt);

caseCreateCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var command = new CreateCaseCommand(
        result.GetValue(caseSystemOpt)!,
        result.GetValue(problemOpt)!,
        result.GetValue(analysisOpt) ?? string.Empty,
        result.GetValue(resolutionOpt) ?? string.Empty,
        result.GetValue(lessonsOpt) ?? string.Empty,
        result.GetValue(ritmOpt),
        result.GetValue(changeOpt));

    var record = mediator.Send(command).Result;

    AnsiConsole.MarkupLine($"[green]Case record created:[/] {record.Id}");
    AnsiConsole.MarkupLine($"  [bold]System:[/] {record.System}");
    AnsiConsole.MarkupLine($"  [bold]Problem:[/] {record.Problem}");
    AnsiConsole.MarkupLine($"  [bold]Status:[/] {record.Status}");
});

caseCmd.Subcommands.Add(caseCreateCmd);

/// <summary>
/// Subcommand: Displays detailed information for a single case record by its identifier.
/// </summary>
var caseShowCmd = new Command("show", "Show case record details");
/// <summary>The unique identifier (GUID) of the case record to display.</summary>
var caseIdArg = new Argument<Guid>("id");
caseShowCmd.Arguments.Add(caseIdArg);
caseShowCmd.SetAction((ParseResult result) =>
{
    using var scope = host.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    var record = mediator.Send(new GetCaseByIdQuery(result.GetValue(caseIdArg))).Result;

    if (record is null)
    {
        AnsiConsole.MarkupLine("[red]Case record not found.[/]");
        return;
    }

    var panel = new Panel(
        new Markup(
            $"[bold]System:[/] {record.System}\n" +
            $"[bold]Problem:[/] {record.Problem}\n" +
            $"[bold]Analysis:[/] {record.Analysis}\n" +
            $"[bold]Resolution:[/] {record.Resolution}\n" +
            $"[bold]Lessons Learned:[/] {record.LessonsLearned}\n" +
            $"[bold]RITM ID:[/] {record.RitmId}\n" +
            $"[bold]Change ID:[/] {record.ChangeId}\n" +
            $"[bold]Status:[/] {record.Status}\n" +
            $"[bold]Created:[/] {record.CreatedAt:yyyy-MM-dd HH:mm:ss}"))
    {
        Header = new PanelHeader($"Case Record - {record.Id}")
    };

    AnsiConsole.Write(panel);
});

caseCmd.Subcommands.Add(caseShowCmd);
rootCommand.Subcommands.Add(caseCmd);

var parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();
