using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Axiom.Application.Interfaces;
using Axiom.Cli.Helpers;
using Axiom.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Axiom.Cli.Commands;

internal static class SyncCommands
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static Command Create(IHost host)
    {
        var syncCmd = new Command("sync", "Sync local JSON store to database");

        var dryRunOpt = new Option<bool>("--dry-run")
        {
            Description = "Show what would be synced without making changes"
        };
        syncCmd.Options.Add(dryRunOpt);

        syncCmd.SetAction((ParseResult result) =>
        {
            using var scope = host.Services.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
            var dryRun = result.GetValue(dryRunOpt);

            if (syncService is EfSyncService efSync)
            {
                efSync.SetConflictResolver(ResolveConflict);
            }

            try
            {
                var ok = syncService.SyncAsync(dryRun, CancellationToken.None).Result;
                if (!ok)
                {
                    Environment.ExitCode = 1;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Sync error: {ex.Message}[/]");
                Environment.ExitCode = 1;
            }
        });

        return syncCmd;
    }

    private static ConflictAction ResolveConflict(SyncConflictInfo info)
    {
        var localJson = JsonSerializer.Serialize(info.LocalEntry, JsonOptions);
        var dbJson = JsonSerializer.Serialize(info.DbEntry, JsonOptions);

        AnsiConsole.MarkupLine($"[yellow]Conflict:[/] {info.EntityType} '{info.Identifier}' already exists in database.");
        AnsiConsole.MarkupLine($"  [bold]Local:[/] {localJson}");
        AnsiConsole.MarkupLine($"  [bold]DB:[/]    {dbJson}");

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What do you want to do?")
                .PageSize(4)
                .AddChoices("Skip", "Update", "Always Skip", "Always Update"));

        return choice switch
        {
            "Skip" => ConflictAction.Skip,
            "Update" => ConflictAction.Update,
            "Always Skip" => ConflictAction.AlwaysSkip,
            "Always Update" => ConflictAction.AlwaysUpdate,
            _ => ConflictAction.Skip
        };
    }
}
