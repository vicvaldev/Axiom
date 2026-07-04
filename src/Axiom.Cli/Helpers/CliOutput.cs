using System.CommandLine;
using System.Text.Json;
using Axiom.Application.Dtos;
using Axiom.Infrastructure.Persistence;
using Spectre.Console;

namespace Axiom.Cli.Helpers;

internal static class CliOutput
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static Option<bool> NewJsonOption() => new("--json")
    {
        Description = "Write machine-readable JSON output"
    };

    public static Option<bool> NewWizardOption() => new("--wizard")
    {
        Description = "Launch interactive wizard to create the entry"
    };

    public static void WriteJson(object value)
    {
        Console.WriteLine(JsonSerializer.Serialize(value, JsonOptions));
    }

    public static void WriteError(string message, bool json)
    {
        Environment.ExitCode = 1;
        if (json)
        {
            WriteJson(new { error = message });
            return;
        }

        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }

    public static JsonStore? CreateJsonStore()
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

    public static void WriteReferenceList(IEnumerable<ReferenceCodeDto> references, bool json, params string[] columns)
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
}
