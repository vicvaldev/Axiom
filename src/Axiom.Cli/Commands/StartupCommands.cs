using System.CommandLine;
using System.CommandLine.Parsing;
using Axiom.Application.Interfaces;
using Axiom.Cli.Helpers;
using Axiom.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Axiom.Cli.Commands;

internal static class StartupCommands
{
    public static Command Create(IHost host)
    {
        var startupCmd = new Command("startup", "Initialize reference data (Users, Systems, KnowledgeTypes, IssueStates, KnowledgeStates)");
        var startupDemoOpt = new Option<bool>("--demo")
        {
            Description = "Seed idempotent demo data without prompts"
        };
        var startupJsonOpt = CliOutput.NewJsonOption();
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
                    CliOutput.WriteJson(svc.SeedDemoDataAsync(ct).Result);
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
                CliOutput.WriteError("Use --json together with --demo for non-interactive startup output.", true);
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

        return startupCmd;
    }
}
